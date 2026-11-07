using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.Monitoring.Enums;

namespace LabBridge.Domain.Features.Monitoring.Entities;

/// <summary>
/// An operational alert. OrganizationId is null for platform-scope alerts (documented tenant exception). At most one Open/Acknowledged alert per
/// (organization, rule, scope key); repeats increment OccurrenceCount. Affected objects are identifiers only (no content).
/// Aggregate root of its state history.
/// </summary>
public sealed class Alert : AggregateRoot
{
    private readonly List<AlertStateHistory> _history = [];

    private Alert() { }

    public Guid? OrganizationId { get; private set; }
    public Guid AlertRuleId { get; private set; }
    public string ScopeKey { get; private set; } = null!;
    public AlertSeverity Severity { get; private set; }
    public AlertStatus Status { get; private set; }
    public int OccurrenceCount { get; private set; }
    public string? AffectedObjectsJson { get; private set; }
    public bool IsSimulated { get; private set; }
    public DateTimeOffset OpenedAt { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; }
    public DateTimeOffset? AcknowledgedAt { get; private set; }
    public Guid? AcknowledgedBy { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }
    public ResolveReason? ResolveReason { get; private set; }
    public string? ResolveNote { get; private set; }
    public Guid? ResolvedBy { get; private set; }

    public IReadOnlyCollection<AlertStateHistory> History => _history.AsReadOnly();

    public static Alert Open(
        AlertRule rule,
        Guid? organizationId,
        string scopeKey,
        AlertSeverity severity,
        string? affectedObjectsJson,
        bool isSimulated,
        DateTimeOffset now
    )
    {
        ArgumentNullException.ThrowIfNull(rule);
        if (rule.Scope == AlertScope.Organization == (organizationId is null))
            throw new ValidationException(
                "ALERT_SCOPE_MISMATCH",
                "Alert scope mismatch",
                "Organization-scope alerts require an organization; platform alerts must not have one."
            );
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmptyWhenSet(organizationId, nameof(organizationId)),
            AlertRuleId = rule.Id,
            ScopeKey = Guard.Required(scopeKey, nameof(scopeKey), 200),
            Severity = Guard.Defined(severity, nameof(severity)),
            Status = AlertStatus.Open,
            OccurrenceCount = 1,
            //AffectedObjectsJson = affectedObjectsJson,
            AffectedObjectsJson = Guard.OptionalJson(affectedObjectsJson, nameof(affectedObjectsJson)),
            IsSimulated = isSimulated,
            OpenedAt = now,
            LastSeenAt = now,
        };
        alert._history.Add(
            new AlertStateHistory(alert.Id, null, AlertStatus.Open, null, null, now)
        );
        return alert;
    }

    public void RecordOccurrence(
        AlertSeverity severity,
        string? affectedObjectsJson,
        DateTimeOffset now
    )
    {
        if (Status == AlertStatus.Resolved)
            throw new BusinessRuleException(
                "ALERT_RESOLVED",
                "Alert resolved",
                "A resolved alert cannot receive occurrences; a new alert must be opened."
            );
        OccurrenceCount++;
        if (severity > Severity)
            Severity = severity;
        //AffectedObjectsJson = affectedObjectsJson ?? AffectedObjectsJson;
        AffectedObjectsJson = Guard.OptionalJson(affectedObjectsJson, nameof(affectedObjectsJson)) ?? AffectedObjectsJson;
        if (now > LastSeenAt)
            LastSeenAt = now;
    }

    public void Acknowledge(Guid actorId, DateTimeOffset now)
    {
        if (Status == AlertStatus.Acknowledged)
            return;
        if (Status != AlertStatus.Open)
            throw new BusinessRuleException(
                "ALERT_INVALID_STATE",
                "Invalid alert state",
                "Only an open alert can be acknowledged."
            );
        Status = AlertStatus.Acknowledged;
        AcknowledgedAt = now;
        AcknowledgedBy = Guard.NotEmpty(actorId, nameof(actorId));
        _history.Add(
            new AlertStateHistory(
                Id,
                AlertStatus.Open,
                AlertStatus.Acknowledged,
                null,
                actorId,
                now
            )
        );
    }

    /// <summary>Resolves the alert. Manual resolution requires a note; automatic reasons are system actions.</summary>
    public void Resolve(ResolveReason reason, string? note, Guid? actorId, DateTimeOffset now)
    {
        if (Status == AlertStatus.Resolved)
            return;
        var from = Status;
        //if (reason == Enums.ResolveReason.Manual && string.IsNullOrWhiteSpace(note))
        //    throw new ValidationException(
        //        "ALERT_RESOLVE_NOTE_REQUIRED",
        //        "Resolve note required",
        //        "Manual resolution requires a note."
        //    );
        if (reason == Enums.ResolveReason.Manual)
        {
            if (string.IsNullOrWhiteSpace(note))
                throw new ValidationException(
                    "ALERT_RESOLVE_NOTE_REQUIRED", "Resolve note required", "Manual resolution requires a note.");
            if (actorId is null)
                throw new ValidationException(
                    "ALERT_RESOLVE_ACTOR_REQUIRED", "Resolving user required", "Manual resolution must record the user.");
        }
        Status = AlertStatus.Resolved;
        ResolveReason = Guard.Defined(reason, nameof(reason));
        ResolveNote = Guard.Optional(note, nameof(note), 500);
        ResolvedAt = now;
        ResolvedBy = actorId;
        _history.Add(
            new AlertStateHistory(
                Id,
                from,
                AlertStatus.Resolved,
                ResolveNote ?? reason.ToString(),
                actorId,
                now
            )
        );
    }
}

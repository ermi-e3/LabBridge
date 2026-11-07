using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.DeliveryManagement.Enums;

namespace LabBridge.Domain.Features.DeliveryManagement.Entities;

/// <summary>Case opened for an Uncertain attempt; resolution requires evidence. One Open case per attempt. Escalates after the route's escalation period.</summary>
public sealed class ReconciliationCase : AuditedAggregateRoot, IOrganizationOwned
{
    private ReconciliationCase() { }

    public Guid OrganizationId { get; private set; }
    public Guid DeliveryId { get; private set; }
    public Guid AttemptId { get; private set; }
    public ReconciliationCaseStatus Status { get; private set; }
    public ReconciliationResolution? Resolution { get; private set; }
    public string? ResolutionReason { get; private set; }
    public string? EvidenceNote { get; private set; }
    public Guid? ResolvedBy { get; private set; }
    public DateTimeOffset OpenedAt { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }
    public DateTimeOffset? EscalatedAt { get; private set; }

    //public static ReconciliationCase Open(DeliveryItem item, Guid attemptId, DateTimeOffset now)
    //{
    //    ArgumentNullException.ThrowIfNull(item);
    //    if (item.State != DeliveryState.Uncertain)
    //        throw new BusinessRuleException(
    //            "RECONCILIATION_NOT_UNCERTAIN",
    //            "Delivery not uncertain",
    //            "A reconciliation case requires an uncertain delivery."
    //        );
    //    var c = new ReconciliationCase
    //    {
    //        Id = Guid.NewGuid(),
    //        OrganizationId = item.OrganizationId,
    //        DeliveryId = item.Id,
    //        AttemptId = Guard.NotEmpty(attemptId, nameof(attemptId)),
    //        Status = ReconciliationCaseStatus.Open,
    //        OpenedAt = now,
    //    };
    //    c.StampCreated(now, null);
    //    return c;
    //}

    public static ReconciliationCase Open(DeliveryItem item, DeliveryAttempt attempt, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(attempt);
        if (item.State != DeliveryState.Uncertain)
            throw new BusinessRuleException("RECONCILIATION_NOT_UNCERTAIN", "Delivery not uncertain",
                "A reconciliation case requires an uncertain delivery.");
        if (attempt.DeliveryId != item.Id || attempt.AttemptResult != AttemptResult.Uncertain)
            throw new BusinessRuleException("RECONCILIATION_ATTEMPT_MISMATCH", "Attempt mismatch",
                "The attempt must be the uncertain attempt of this delivery.");
        var c = new ReconciliationCase
        {
            Id = Guid.NewGuid(),
            OrganizationId = item.OrganizationId,
            DeliveryId = item.Id,
            AttemptId = attempt.Id,
            Status = ReconciliationCaseStatus.Open,
            OpenedAt = now,
        };
        c.StampCreated(now, null);
        return c;
    }

    public void Escalate(DateTimeOffset now)
    {
        if (Status != ReconciliationCaseStatus.Open)
            throw new BusinessRuleException(
                "RECONCILIATION_INVALID_STATE",
                "Invalid case state",
                "Only an open case can be escalated."
            );
        Status = ReconciliationCaseStatus.Escalated;
        EscalatedAt = now;
        StampUpdated(now, null);
    }

    public void Resolve(
        ReconciliationResolution resolution,
        string reason,
        string? evidenceNote,
        Guid resolvedBy,
        DateTimeOffset now
    )
    {
        if (Status == ReconciliationCaseStatus.Resolved)
            throw new BusinessRuleException(
                "RECONCILIATION_INVALID_STATE",
                "Invalid case state",
                "The case is already resolved."
            );
        Resolution = Guard.Defined(resolution, nameof(resolution));
        ResolutionReason = Guard.Required(reason, nameof(reason), 500);
        EvidenceNote = Guard.Optional(evidenceNote, nameof(evidenceNote), 1000);
        ResolvedBy = Guard.NotEmpty(resolvedBy, nameof(resolvedBy));
        ResolvedAt = now;
        Status = ReconciliationCaseStatus.Resolved;
        StampUpdated(now, resolvedBy);
    }
}

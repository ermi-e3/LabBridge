using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.Audit.Enums;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace LabBridge.Domain.Features.Audit.Entities;

/// <summary>
/// Append-only audit record (no update/delete path; the database rejects both except the sealing columns, which are set once by the sealer).
/// OrganizationId is null for the platform stream. Detail must follow the action's detail schema and never contain secrets or clinical content.
/// ContentHash is SHA-256 over the canonical serialization of the stored fields (ASSUMPTION A-AUD-02: field order and '|' separators defined below).
/// </summary>
public sealed class AuditEvent : Entity
{
    private AuditEvent() { }

    public Guid? OrganizationId { get; private set; }
    public string ActionCode { get; private set; } = null!;
    public AuditScope Scope { get; private set; }
    public long? StreamSequence { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }
    public AuditActorType ActorType { get; private set; }
    public string? ActorId { get; private set; }
    public string? TargetType { get; private set; }
    public string? TargetId { get; private set; }
    public AuditOutcome Outcome { get; private set; }
    public Guid? CorrelationId { get; private set; }
    public string? TraceId { get; private set; }
    public string? RequestId { get; private set; } 
    public string? RelatedIdsJson { get; private set; }
    public string? SourceAddress { get; private set; }
    public string? Reason { get; private set; }
    public string? DetailJson { get; private set; }
    public bool IsSimulated { get; private set; }
    public bool TimeSkewFlag { get; private set; }
    public int SchemaVersion { get; private set; }
    public string ContentHash { get; private set; } = null!;
    public Guid? SealId { get; private set; }

    public static AuditEvent Record(
        AuditActionDefinition action,
        Guid? organizationId,
        AuditActorType actorType,
        string? actorId,
        string? targetType,
        string? targetId,
        AuditOutcome outcome,
        Guid? correlationId,
        string? traceId,
        string? requestId,
        string? relatedIdsJson,
        string? sourceAddress,
        string? reason,
        string? detailJson,
        bool isSimulated,
        bool timeSkewFlag,
        int schemaVersion,
        DateTimeOffset occurredAt,
        DateTimeOffset recordedAt
    )
    {
        ArgumentNullException.ThrowIfNull(action);
        if (!action.IsActive)
            throw new BusinessRuleException(
                "AUDIT_ACTION_INACTIVE",
                "Audit action inactive",
                "The audit action definition is not active."
            );
        var scope = organizationId is null ? AuditScope.Platform : AuditScope.Organization;
        var e = new AuditEvent
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmptyWhenSet(organizationId, nameof(organizationId)),
            ActionCode = action.ActionCode,
            Scope = scope,
            OccurredAt = Normalize(occurredAt),
            RecordedAt = Normalize(recordedAt),
            ActorType = Guard.Defined(actorType, nameof(actorType)),
            ActorId = Guard.Optional(actorId, nameof(actorId), 100),
            TargetType = Guard.Optional(targetType, nameof(targetType), 50),
            TargetId = Guard.Optional(targetId, nameof(targetId), 100),
            Outcome = Guard.Defined(outcome, nameof(outcome)),
            CorrelationId = correlationId,
            TraceId = Guard.Optional(traceId, nameof(traceId), 64),
            RequestId = Guard.Optional(requestId, nameof(requestId), 100),
            RelatedIdsJson = relatedIdsJson,
            SourceAddress = Guard.Optional(sourceAddress, nameof(sourceAddress), 64),
            Reason = Guard.Optional(reason, nameof(reason), 500),
            DetailJson = detailJson,
            IsSimulated = isSimulated,
            TimeSkewFlag = timeSkewFlag,
            SchemaVersion = Guard.InRange(schemaVersion, 1, int.MaxValue, nameof(schemaVersion)),
        };
        e.ContentHash = e.ComputeContentHash();
        return e;
    }

    /// <summary>Sets the sealing columns exactly once (called by the sealer only).</summary>
    //public void Seal(long streamSequence, Guid sealId)
    //{
    //    if (SealId is not null)
    //        throw new BusinessRuleException(
    //            "AUDIT_EVENT_SEALED",
    //            "Event already sealed",
    //            "Audit events are write-once."
    //        );
    //    StreamSequence = Guard.NonNegative(streamSequence, nameof(streamSequence));
    //    SealId = Guard.NotEmpty(sealId, nameof(sealId));
    //}

    public void Seal(AuditSeal seal, long streamSequence)
    {
        ArgumentNullException.ThrowIfNull(seal);
        if (SealId is not null)
            throw new BusinessRuleException("AUDIT_EVENT_SEALED", "Event already sealed", "Audit events are write-once.");
        if (seal.OrganizationId != OrganizationId)
            throw new BusinessRuleException("AUDIT_SEAL_STREAM_MISMATCH", "Seal stream mismatch", "The seal belongs to a different audit stream.");
        if (streamSequence < seal.FromEventSequence || streamSequence > seal.ToEventSequence)
            throw new ValidationException("AUDIT_SEQUENCE_OUT_OF_RANGE", "Sequence out of range", "The sequence is outside the range of the seal.");
        StreamSequence = streamSequence;
        SealId = seal.Id;
    }

    /// <summary>Recomputes the canonical hash (used by integrity verification).</summary>
    //public string ComputeContentHash()
    //{
    //    var canonical = string.Join(
    //        '|',
    //        Id,
    //        OrganizationId,
    //        ActionCode,
    //        Scope,
    //        OccurredAt.UtcDateTime.ToString("O"),
    //        RecordedAt.UtcDateTime.ToString("O"),
    //        ActorType,
    //        ActorId,
    //        TargetType,
    //        TargetId,
    //        Outcome,
    //        CorrelationId,
    //        TraceId,
    //        RequestId,
    //        RelatedIdsJson,
    //        SourceAddress,
    //        Reason,
    //        DetailJson,
    //        IsSimulated,
    //        TimeSkewFlag,
    //        SchemaVersion
    //    );
    //    return Convert
    //        .ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))
    //        .ToLowerInvariant();
    //}

    public string ComputeContentHash()
    {
        var sb = new StringBuilder();
        Append(sb, Id.ToString());
        Append(sb, OrganizationId?.ToString());
        Append(sb, ActionCode);
        Append(sb, Scope.ToString());
        Append(sb, OccurredAt.UtcTicks.ToString(CultureInfo.InvariantCulture));
        Append(sb, RecordedAt.UtcTicks.ToString(CultureInfo.InvariantCulture));
        Append(sb, ActorType.ToString());
        Append(sb, ActorId);
        Append(sb, TargetType);
        Append(sb, TargetId);
        Append(sb, Outcome.ToString());
        Append(sb, CorrelationId?.ToString());
        Append(sb, TraceId);
        Append(sb, RequestId);
        Append(sb, RelatedIdsJson);
        Append(sb, SourceAddress);
        Append(sb, Reason);
        Append(sb, DetailJson);
        Append(sb, IsSimulated ? "1" : "0");
        Append(sb, TimeSkewFlag ? "1" : "0");
        Append(sb, SchemaVersion.ToString(CultureInfo.InvariantCulture));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()))).ToLowerInvariant();
    }

    // length-prefixed, null distinguished from empty: no ambiguity between field boundaries
    private static void Append(StringBuilder sb, string? value)
    {
        if (value is null) sb.Append("~;");
        else sb.Append(value.Length).Append(':').Append(value).Append(';');
    }


    public bool VerifyIntegrity() => string.Equals(ContentHash, ComputeContentHash(), StringComparison.Ordinal);

    private static DateTimeOffset Normalize(DateTimeOffset value)
    {
        var utc = value.ToUniversalTime();
        // PostgreSQL timestamptz keeps microseconds; 1 microsecond = 10 ticks
        return new DateTimeOffset(utc.Ticks - utc.Ticks % 10, TimeSpan.Zero);
    }
}

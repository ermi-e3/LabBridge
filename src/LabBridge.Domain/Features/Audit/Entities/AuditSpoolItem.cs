using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.Audit.Enums;

namespace LabBridge.Domain.Features.Audit.Entities;

/// <summary>Durable buffer entry for a Buffered audit event until it is delivered to the audit store. Contains the event JSON (same classification as AuditEvent).</summary>
public sealed class AuditSpoolItem : Entity
{
    private AuditSpoolItem() { }

    public Guid AuditEventId { get; private set; }
    public string EventJson { get; private set; } = null!;
    public SpoolStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTimeOffset NextAttemptAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static AuditSpoolItem Enqueue(Guid auditEventId, string eventJson, DateTimeOffset now) =>
        new()
        {
            Id = Guid.NewGuid(),
            AuditEventId = Guard.NotEmpty(auditEventId, nameof(auditEventId)),
            EventJson = Guard.RequiredExact(eventJson, nameof(eventJson), int.MaxValue),
            Status = SpoolStatus.Pending,
            NextAttemptAt = now,
            CreatedAt = now,
        };

    public void MarkDelivered()
    {
        Require(Status == SpoolStatus.Pending);
        Status = SpoolStatus.Delivered;
    }

    /// <summary>Records a failed delivery; after <paramref name="maxAttempts"/> the item is Dead (never silently dropped).</summary>
    public void RegisterFailure(int maxAttempts, DateTimeOffset nextAttemptAt)
    {
        Require(Status == SpoolStatus.Pending);
        AttemptCount++;
        if (AttemptCount >= Guard.InRange(maxAttempts, 1, 1000, nameof(maxAttempts)))
            Status = SpoolStatus.Dead;
        else
            NextAttemptAt = nextAttemptAt;
    }

    private static void Require(bool condition)
    {
        if (!condition)
            throw new BusinessRuleException(
                "AUDIT_SPOOL_INVALID_STATE",
                "Invalid spool state",
                "Only a pending spool item can change."
            );
    }
}

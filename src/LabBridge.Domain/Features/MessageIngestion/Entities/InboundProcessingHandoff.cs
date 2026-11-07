using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.MessageIngestion.Enums;

namespace LabBridge.Domain.Features.MessageIngestion.Entities;

/// <summary>
/// Persistent, lease-based hand-off from ingestion to processing, committed with the message (BR-ING-011). One per message.
/// At-least-once: an expired lease returns the item to Pending, consumers are idempotent. Held items are released in ReceiptSequence order (BR-ING-010).
/// </summary>
public sealed class InboundProcessingHandoff : AuditedAggregateRoot, IOrganizationOwned
{
    private InboundProcessingHandoff() { }

    public Guid OrganizationId { get; private set; }
    public Guid MessageId { get; private set; }
    public HandoffStatus Status { get; private set; }
    public string? HoldReason { get; private set; }
    public long ReceiptSequence { get; private set; }
    public DateTimeOffset AvailableAt { get; private set; }
    public string? ClaimedBy { get; private set; }
    public DateTimeOffset? LeaseExpiresAt { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    /// <summary>Created Pending, or Held when organization policy requires holding (HoldAndPreserve). ReceiptSequence is known after the message insert.</summary>
    public static InboundProcessingHandoff Create(
        InboundMessage message,
        long receiptSequence,
        string? holdReason,
        DateTimeOffset now
    )
    {
        ArgumentNullException.ThrowIfNull(message);
        if (receiptSequence <= 0)
            throw new ValidationException(
                "HANDOFF_SEQUENCE_REQUIRED",
                "Receipt sequence required",
                "The receipt sequence must be assigned before the hand-off is created."
            );
        if (message.Status != MessageStatus.Received)
            throw new BusinessRuleException(
                "HANDOFF_INVALID_STATE",
                "Invalid hand-off state",
                "A hand-off is only created for a message in status Received."
            );
        var handoff = new InboundProcessingHandoff
        {
            Id = Guid.NewGuid(),
            OrganizationId = message.OrganizationId,
            MessageId = message.Id,
            ReceiptSequence = receiptSequence,
            Status = holdReason is null ? HandoffStatus.Pending : HandoffStatus.Held,
            HoldReason = Guard.Optional(holdReason, nameof(holdReason), 50),
            AvailableAt = now,
        };
        handoff.StampCreated(now, null);
        return handoff;
    }

    public void Hold(string reason, DateTimeOffset now)
    {
        Require(Status == HandoffStatus.Pending, "Only a pending hand-off can be held.");
        HoldReason = Guard.Required(reason, nameof(reason), 50);
        Status = HandoffStatus.Held;
        StampUpdated(now, null);
    }

    public void ReleaseHold(DateTimeOffset now)
    {
        Require(Status == HandoffStatus.Held, "Only a held hand-off can be released.");
        HoldReason = null;
        Status = HandoffStatus.Pending;
        AvailableAt = now;
        StampUpdated(now, null);
    }

    public void Claim(string claimedBy, DateTimeOffset leaseExpiresAt, DateTimeOffset now)
    {
        Require(
            Status == HandoffStatus.Pending && AvailableAt <= now,
            "Only an available pending hand-off can be claimed."
        );
        if (leaseExpiresAt <= now)
            throw new ValidationException(
                "HANDOFF_LEASE_INVALID",
                "Invalid lease",
                "The lease must expire in the future."
            );
        ClaimedBy = Guard.Required(claimedBy, nameof(claimedBy), 100);
        LeaseExpiresAt = leaseExpiresAt;
        AttemptCount++;
        Status = HandoffStatus.Claimed;
        StampUpdated(now, null);
    }

    /// <summary>Claimed -> Pending (explicit release or lease expiry).</summary>
    public void ReleaseClaim(DateTimeOffset now)
    {
        Require(Status == HandoffStatus.Claimed, "Only a claimed hand-off can be released.");
        ClearLease();
        Status = HandoffStatus.Pending;
        AvailableAt = now;
        StampUpdated(now, null);
    }

    public bool IsLeaseExpired(DateTimeOffset now) =>
        Status == HandoffStatus.Claimed && LeaseExpiresAt <= now;

    public void Complete(DateTimeOffset now)
    {
        Require(Status == HandoffStatus.Claimed, "Only a claimed hand-off can be completed.");
        ClearLease();
        Status = HandoffStatus.Completed;
        CompletedAt = now;
        StampUpdated(now, null);
    }

    /// <summary>Any non-completed hand-off can be cancelled (ConfirmDuplicate).</summary>
    //public void Cancel(DateTimeOffset now)
    //{
    //    Require(Status != HandoffStatus.Completed, "A completed hand-off cannot be cancelled.");
    //    if (Status == HandoffStatus.Cancelled)
    //        return;
    //    ClearLease();
    //    Status = HandoffStatus.Cancelled;
    //    StampUpdated(now, null);
    //}

    /// <summary>Any non-completed hand-off can be cancelled (ConfirmDuplicate).</summary>
    public void Cancel(DateTimeOffset now)
    {
        Require(Status != HandoffStatus.Completed, "A completed hand-off cannot be cancelled.");
        if (Status == HandoffStatus.Cancelled)
            return;
        ClearLease();
        HoldReason = null;            // required by ck_handoffs_hold: hold_reason is set only while Held
        Status = HandoffStatus.Cancelled;
        StampUpdated(now, null);
    }

    private void ClearLease()
    {
        ClaimedBy = null;
        LeaseExpiresAt = null;
    }

    private static void Require(bool condition, string detail)
    {
        if (!condition)
            throw new BusinessRuleException(
                "HANDOFF_INVALID_STATE",
                "Invalid hand-off state",
                detail
            );
    }
}

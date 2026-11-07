using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using System.Text.Json;
using LabBridge.Domain.Features.MessageProcessing.Enums;

namespace LabBridge.Domain.Features.MessageProcessing.Entities;

/// <summary>Lease-based hand-off of a normalized message to Workflow or Delivery, committed with the message (one per normalized message).</summary>
public sealed class NormalizedHandoff : AuditedAggregateRoot, IOrganizationOwned
{
    private NormalizedHandoff() { }

    public Guid OrganizationId { get; private set; }
    public Guid NormalizedMessageId { get; private set; }
    public ConsumerKind Consumer { get; private set; }
    public NormalizedHandoffStatus Status { get; private set; }
    public long ReceiptSequence { get; private set; }
    public DateTimeOffset AvailableAt { get; private set; }
    public string? ClaimedBy { get; private set; }
    public DateTimeOffset? LeaseExpiresAt { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public static NormalizedHandoff Create(
        NormalizedMessage message,
        long receiptSequence,
        DateTimeOffset now
    )
    {
        ArgumentNullException.ThrowIfNull(message);
        var handoff = new NormalizedHandoff
        {
            Id = Guid.NewGuid(),
            OrganizationId = message.OrganizationId,
            NormalizedMessageId = message.Id,
            Consumer = message.Consumer,
            ReceiptSequence =
                receiptSequence > 0
                    ? receiptSequence
                    : throw new ValidationException(
                        "HANDOFF_SEQUENCE_REQUIRED",
                        "Receipt sequence required",
                        "The receipt sequence of the source message is required."
                    ),
            Status = NormalizedHandoffStatus.Pending,
            AvailableAt = now,
        };
        handoff.StampCreated(now, null);
        return handoff;
    }

    public void Claim(string claimedBy, DateTimeOffset leaseExpiresAt, DateTimeOffset now)
    {
        if (Status != NormalizedHandoffStatus.Pending || AvailableAt > now)
            throw Invalid("Only an available pending hand-off can be claimed.");
        if (leaseExpiresAt <= now)
            throw new ValidationException(
                "HANDOFF_LEASE_INVALID",
                "Invalid lease",
                "The lease must expire in the future."
            );
        ClaimedBy = Guard.Required(claimedBy, nameof(claimedBy), 100);
        LeaseExpiresAt = leaseExpiresAt;
        AttemptCount++;
        Status = NormalizedHandoffStatus.Claimed;
        StampUpdated(now, null);
    }

    public void ReleaseClaim(DateTimeOffset now)
    {
        if (Status != NormalizedHandoffStatus.Claimed)
            throw Invalid("Only a claimed hand-off can be released.");
        ClaimedBy = null;
        LeaseExpiresAt = null;
        Status = NormalizedHandoffStatus.Pending;
        AvailableAt = now;
        StampUpdated(now, null);
    }

    public void Complete(DateTimeOffset now)
    {
        if (Status != NormalizedHandoffStatus.Claimed)
            throw Invalid("Only a claimed hand-off can be completed.");
        ClaimedBy = null;
        LeaseExpiresAt = null;
        Status = NormalizedHandoffStatus.Completed;
        CompletedAt = now;
        StampUpdated(now, null);
    }

    public void Cancel(DateTimeOffset now)
    {
        if (Status == NormalizedHandoffStatus.Completed)
            throw Invalid("A completed hand-off cannot be cancelled.");
        if (Status == NormalizedHandoffStatus.Cancelled)
            return;
        ClaimedBy = null;
        LeaseExpiresAt = null;
        Status = NormalizedHandoffStatus.Cancelled;
        StampUpdated(now, null);
    }

    private static BusinessRuleException Invalid(string detail) =>
        new("HANDOFF_INVALID_STATE", "Invalid hand-off state", detail);
}

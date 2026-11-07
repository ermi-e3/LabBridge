using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Common.ValueObjects;

namespace LabBridge.Domain.Features.Audit.Entities;

/// <summary>Hash-chained block over a contiguous range of audit events of one stream; retained as an anchor even after the events are purged.</summary>
public sealed class AuditSeal : Entity
{
    private AuditSeal() { }

    public Guid? OrganizationId { get; private set; }
    public long SealSequence { get; private set; }
    public long FromEventSequence { get; private set; }
    public long ToEventSequence { get; private set; }
    public int EventCount { get; private set; }
    public string BlockHash { get; private set; } = null!;
    public string? PreviousBlockHash { get; private set; }
    public DateTimeOffset SealedAt { get; private set; }

    /// <summary>Creates the next seal; enforces gap-free sequences and the hash chain against the previous seal (null for the first).</summary>
    public static AuditSeal Create(
        AuditSeal? previous,
        Guid? organizationId,
        long fromEventSequence,
        long toEventSequence,
        int eventCount,
        string blockHash,
        DateTimeOffset now
    )
    {
        if (toEventSequence < fromEventSequence || eventCount < 1)
            throw new ValidationException(
                "AUDIT_SEAL_RANGE_INVALID",
                "Invalid seal range",
                "The sealed range must contain at least one event."
            );
        if (previous is null)
        {
            if (fromEventSequence != 1)
                throw new BusinessRuleException(
                    "AUDIT_SEAL_CHAIN_BROKEN",
                    "Seal chain broken",
                    "The first seal must start at event sequence 1."
                );
        }
        else if (
            previous.OrganizationId != organizationId
            || fromEventSequence != previous.ToEventSequence + 1
        )
        {
            throw new BusinessRuleException(
                "AUDIT_SEAL_CHAIN_BROKEN",
                "Seal chain broken",
                "The seal must continue directly after the previous seal of the same stream."
            );
        }
        // change 
        if (eventCount != toEventSequence - fromEventSequence + 1)
            throw new ValidationException("AUDIT_SEAL_COUNT_MISMATCH", "Event count mismatch",
                "The event count must equal the size of the sequence range.");
        // ...

        return new AuditSeal
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmptyWhenSet(organizationId, nameof(organizationId)),
            SealSequence = (previous?.SealSequence ?? 0) + 1,
            FromEventSequence = fromEventSequence,
            ToEventSequence = toEventSequence,
            EventCount = eventCount,
            //BlockHash = Guard.Required(blockHash, nameof(blockHash), 64),
            BlockHash = Sha256Hash.Create(blockHash).Value,
            PreviousBlockHash = previous?.BlockHash,
            SealedAt = now,
        };
    }
}

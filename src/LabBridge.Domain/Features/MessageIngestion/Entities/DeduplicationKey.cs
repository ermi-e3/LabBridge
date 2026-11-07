using LabBridge.Domain.Common;
using LabBridge.Domain.Common.ValueObjects;

namespace LabBridge.Domain.Features.MessageIngestion.Entities;

/// <summary>
/// The atomic claim that makes the first message under a (organization, connector, dedup key) win (BR-ING-008).
/// A repeated message is a normal integration condition handled through this claim, not an exception.
/// </summary>
public sealed class DeduplicationKey : Entity, IOrganizationOwned
{
    private DeduplicationKey() { }

    public Guid OrganizationId { get; private set; }
    public Guid ConnectorId { get; private set; }
    public string DedupKey { get; private set; } = null!;
    public string ContentHash { get; private set; } = null!;
    public Guid FirstMessageId { get; private set; }
    public DateTimeOffset FirstSeenAt { get; private set; }

    /// <summary>Null = no expiry (DEC-023).</summary>
    public DateTimeOffset? ExpiresAt { get; private set; }

    public static DeduplicationKey Claim(InboundMessage firstMessage, DateTimeOffset? expiresAt)
    {
        ArgumentNullException.ThrowIfNull(firstMessage);
        return new DeduplicationKey
        {
            Id = Guid.NewGuid(),
            OrganizationId = firstMessage.OrganizationId,
            ConnectorId = firstMessage.ConnectorId,
            DedupKey = firstMessage.DedupKey,
            ContentHash = Sha256Hash.Create(firstMessage.ContentHash).Value,
            FirstMessageId = firstMessage.Id,
            FirstSeenAt = firstMessage.ReceivedAt,
            ExpiresAt = expiresAt,
        };
    }

    /// <summary>True when a later message with the same key carries identical content (exact duplicate) rather than different content (conflict).</summary>
    public bool IsExactDuplicate(Sha256Hash contentHash) =>
        string.Equals(ContentHash, contentHash.Value, StringComparison.Ordinal);
}

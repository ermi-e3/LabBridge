using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Common.ValueObjects;
using System.Security.Cryptography;

namespace LabBridge.Domain.Features.MessageIngestion.Entities;

/// <summary>
/// The preserved original payload. Content is never modified (BR-ING-003); a purge clears content only and keeps hash, size and metadata.
/// Exact duplicates share the payload of the original message. Content must never appear in logs, audit events or exception details (BR-ING-012).
/// </summary>
public sealed class RawPayload : Entity, IOrganizationOwned
{
    private RawPayload() { }

    public Guid OrganizationId { get; private set; }

    /// <summary>Null after purge.</summary>
    public byte[]? Content { get; private set; }
    public string ContentHash { get; private set; } = null!;
    public long SizeBytes { get; private set; }
    public string? EncodingHint { get; private set; }

    /// <summary>For example StorageLevel or ApplicationEncrypted.</summary>
    public string ProtectionScheme { get; private set; } = null!;
    public string? KeyRef { get; private set; }
    public DateTimeOffset StoredAt { get; private set; }
    public DateTimeOffset? PurgedAt { get; private set; }
    public string? PurgeReason { get; private set; }

    public bool IsPurged => PurgedAt is not null;

    /// <summary>Preserves a payload. Empty payloads are refused (BR-ING-015) and recorded as refusals instead.</summary>
    public static RawPayload Preserve(
        Guid organizationId,
        byte[] content,
        Sha256Hash contentHash,
        string? encodingHint,
        string protectionScheme,
        string? keyRef,
        DateTimeOffset now
    )
    {
        if (content is null || content.Length == 0)
            throw new ValidationException(
                "MESSAGE_PAYLOAD_EMPTY",
                "Empty payload",
                "An empty payload cannot be preserved."
            );
        ArgumentNullException.ThrowIfNull(contentHash);

        // added 
        var actual = Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant();
        if (!string.Equals(actual, contentHash.Value, StringComparison.Ordinal))
            throw new ValidationException("MESSAGE_PAYLOAD_HASH_MISMATCH", "Content hash mismatch",
                "The content hash does not match the payload.");

        return new RawPayload
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
            Content = content,
            ContentHash = contentHash.Value,
            SizeBytes = content.LongLength,
            EncodingHint = Guard.Optional(encodingHint, nameof(encodingHint), 50),
            ProtectionScheme = Guard.Required(protectionScheme, nameof(protectionScheme), 50),
            KeyRef = Guard.Optional(keyRef, nameof(keyRef), 200),
            StoredAt = now,
        };
    }

    /// <summary>Clears the content only (BR-ING-017). Eligibility (retention elapsed, not quarantined, hand-off complete) is checked by the caller.</summary>
    public void Purge(string reason, DateTimeOffset now)
    {
        if (IsPurged)
            return;
        PurgeReason = Guard.Required(reason, nameof(reason), 100);
        Content = null;
        PurgedAt = now;
    }
}

using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Common.ValueObjects;
using System.Security.Cryptography;

namespace LabBridge.Domain.Features.DeliveryManagement.Entities;

/// <summary>Payload captured by the outbound simulator for inspection (Restricted: readable only with ingestion.payload.read). Purge clears content only.</summary>
public sealed class SimulatorSinkMessage : Entity, IOrganizationOwned
{
    private SimulatorSinkMessage() { }

    public Guid OrganizationId { get; private set; }
    public Guid ConnectorId { get; private set; }
    public Guid AttemptId { get; private set; }
    public byte[]? Content { get; private set; }
    public string ContentHash { get; private set; } = null!;
    public DateTimeOffset ReceivedAt { get; private set; }
    public DateTimeOffset? PurgedAt { get; private set; }

    public static SimulatorSinkMessage Capture(
        DeliveryAttempt attempt,
        Guid connectorId,
        byte[] content,
        Sha256Hash contentHash,
        DateTimeOffset now
    )
    {
        var actual = Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant();
        if (!string.Equals(actual, contentHash.Value, StringComparison.Ordinal))
            throw new ValidationException("SINK_HASH_MISMATCH", "Content hash mismatch",
                "The content hash does not match the captured content.");

        ArgumentNullException.ThrowIfNull(attempt);
        return new SimulatorSinkMessage
        {
            Id = Guid.NewGuid(),
            OrganizationId = attempt.OrganizationId,
            ConnectorId = Guard.NotEmpty(connectorId, nameof(connectorId)),
            AttemptId = attempt.Id,
            Content = Guard.RequiredBytes(content, nameof(content)),
            ContentHash = contentHash.Value,
            ReceivedAt = now,
        };
    }

    public void Purge(DateTimeOffset now)
    {
        if (PurgedAt is not null)
            return;
        Content = null;
        PurgedAt = now;
    }
}

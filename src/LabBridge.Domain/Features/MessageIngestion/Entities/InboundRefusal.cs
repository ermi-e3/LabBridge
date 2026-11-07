using LabBridge.Domain.Common;
using LabBridge.Domain.Features.MessageIngestion.Enums;

namespace LabBridge.Domain.Features.MessageIngestion.Entities;

/// <summary>Metadata-only record of a message refused after source resolution; holds no payload (BR-ING-002, BR-ING-020). Insert-only.</summary>
public sealed class InboundRefusal : Entity, IOrganizationOwned
{
    private InboundRefusal() { }

    public Guid OrganizationId { get; private set; }
    public Guid ConnectorId { get; private set; }
    public RefusalReason Reason { get; private set; }
    public string? OrganizationStatus { get; private set; }
    public long? SizeBytes { get; private set; }
    public string? ContentHash { get; private set; }
    public string? TransportType { get; private set; }
    public string? RemoteEndpoint { get; private set; }
    public Guid CorrelationId { get; private set; }
    public string? TraceId { get; private set; }
    public DateTimeOffset RefusedAt { get; private set; }

    public static InboundRefusal Record(
        Guid organizationId,
        Guid connectorId,
        RefusalReason reason,
        string? organizationStatus,
        long? sizeBytes,
        string? contentHash,
        string? transportType,
        string? remoteEndpoint,
        Guid correlationId,
        string? traceId,
        DateTimeOffset refusedAt
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
            ConnectorId = Guard.NotEmpty(connectorId, nameof(connectorId)),
            Reason = Guard.Defined(reason, nameof(reason)),
            OrganizationStatus = Guard.Optional(organizationStatus, nameof(organizationStatus), 20),
            SizeBytes = sizeBytes is null
                ? null
                : Guard.NonNegative(sizeBytes.Value, nameof(sizeBytes)),
            ContentHash = Guard.Optional(contentHash, nameof(contentHash), 64),
            TransportType = Guard.Optional(transportType, nameof(transportType), 50),
            RemoteEndpoint = Guard.Optional(remoteEndpoint, nameof(remoteEndpoint), 300),
            CorrelationId = Guard.NotEmpty(correlationId, nameof(correlationId)),
            TraceId = Guard.Optional(traceId, nameof(traceId), 64),
            RefusedAt = refusedAt,
        };
}

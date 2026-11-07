using LabBridge.Domain.Common;
using LabBridge.Domain.Features.InstrumentManagement.Enums;

namespace LabBridge.Domain.Features.Monitoring.Entities;

/// <summary>Append-only record of a connector health change. (ConnectorId, EventKey) is unique, making recording idempotent. No secrets, no patient data.</summary>
public sealed class ConnectorHealthSnapshot : Entity, IOrganizationOwned
{
    private ConnectorHealthSnapshot() { }

    public Guid OrganizationId { get; private set; }
    public Guid ConnectorId { get; private set; }
    public ConnectorHealthStatus Status { get; private set; }
    public ConnectorHealthStatus? PreviousStatus { get; private set; }
    public string? LastErrorCode { get; private set; }
    public int ConfigVersion { get; private set; }
    public int? AppliedConfigVersion { get; private set; }
    public bool IsSimulated { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string EventKey { get; private set; } = null!;

    public static ConnectorHealthSnapshot Record(
        Guid organizationId,
        Guid connectorId,
        ConnectorHealthStatus status,
        ConnectorHealthStatus? previous,
        string? lastErrorCode,
        int configVersion,
        int? appliedConfigVersion,
        bool isSimulated,
        DateTimeOffset occurredAt,
        string eventKey
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
            ConnectorId = Guard.NotEmpty(connectorId, nameof(connectorId)),
            Status = Guard.Defined(status, nameof(status)),
            PreviousStatus = previous,
            LastErrorCode = Guard.Optional(lastErrorCode, nameof(lastErrorCode), 50),
            ConfigVersion = Guard.InRange(configVersion, 1, int.MaxValue, nameof(configVersion)),
            AppliedConfigVersion = appliedConfigVersion,
            IsSimulated = isSimulated,
            OccurredAt = occurredAt,
            EventKey = Guard.Required(eventKey, nameof(eventKey), 200),
        };
}

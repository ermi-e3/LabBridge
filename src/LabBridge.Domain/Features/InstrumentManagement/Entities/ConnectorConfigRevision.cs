using LabBridge.Domain.Common;

namespace LabBridge.Domain.Features.InstrumentManagement.Entities;

/// <summary>Immutable snapshot of a connector configuration version (no secret values, only secret slot versions; BR-CON-016). Insert-only.</summary>
public sealed class ConnectorConfigRevision : Entity, IOrganizationOwned
{
    private ConnectorConfigRevision() { }

    internal ConnectorConfigRevision(
        Guid organizationId,
        Guid connectorId,
        int configVersion,
        string snapshotJson,
        DateTimeOffset changedAt,
        Guid? changedBy,
        string? changeReason
    )
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        ConnectorId = connectorId;
        ConfigVersion = configVersion;
        SnapshotJson = Guard.RequiredExact(snapshotJson, nameof(snapshotJson), int.MaxValue);
        ChangedAt = changedAt;
        ChangedBy = changedBy;
        ChangeReason = Guard.Optional(changeReason, nameof(changeReason), 500);
    }

    public Guid OrganizationId { get; private set; }
    public Guid ConnectorId { get; private set; }
    public int ConfigVersion { get; private set; }
    public string SnapshotJson { get; private set; } = null!;
    public DateTimeOffset ChangedAt { get; private set; }
    public Guid? ChangedBy { get; private set; }
    public string? ChangeReason { get; private set; }
}

using LabBridge.Domain.Common;

namespace LabBridge.Domain.Features.InstrumentManagement.Entities;

/// <summary>Reference (never the value) to a secret held in the external secret store, per connector slot (BR-CON-011).</summary>
public sealed class ConnectorSecretRef : Entity, IOrganizationOwned
{
    private ConnectorSecretRef() { }

    internal ConnectorSecretRef(
        Guid organizationId,
        Guid connectorId,
        string slot,
        string secretReference,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        ConnectorId = connectorId;
        Slot = Guard.Required(slot, nameof(slot), 50);
        Replace(secretReference, now, actorId, 1);
    }

    public Guid OrganizationId { get; private set; }
    public Guid ConnectorId { get; private set; }
    public string Slot { get; private set; } = null!;

    /// <summary>Opaque identifier in the secret store.</summary>
    public string SecretReference { get; private set; } = null!;
    public int Version { get; private set; }
    public DateTimeOffset SetAt { get; private set; }
    public Guid? SetBy { get; private set; }

    internal void Replace(
        string secretReference,
        DateTimeOffset now,
        Guid? actorId,
        int? version = null
    )
    {
        SecretReference = Guard.RequiredExact(secretReference, nameof(secretReference), 300);
        Version = version ?? Version + 1;
        SetAt = now;
        SetBy = actorId;
    }
}

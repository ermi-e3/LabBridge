using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.InstrumentManagement.Enums;

namespace LabBridge.Domain.Features.InstrumentManagement.Entities;

/// <summary>
/// An external laboratory system (LIS/LIMS/HIS) that sends orders to or receives results from LabBridge.
/// Type and IsSimulated are immutable after creation (BR-CON-007). Never deleted (BR-CON-014).
/// </summary>
public sealed class ExternalSystem : AuditedAggregateRoot, IOrganizationOwned
{
    private ExternalSystem() { }

    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = null!;
    public ExternalSystemType SystemType { get; private set; }
    public string? Description { get; private set; }
    public bool IsSimulated { get; private set; }
    public ActiveStatus Status { get; private set; }

    public static ExternalSystem Register(
        Guid organizationId,
        string name,
        ExternalSystemType systemType,
        string? description,
        bool isSimulated,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        var system = new ExternalSystem
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
            Name = Guard.Required(name, nameof(name), 200),
            SystemType = Guard.Defined(systemType, nameof(systemType)),
            Description = Guard.Optional(description, nameof(description), 1000),
            IsSimulated = isSimulated,
            Status = ActiveStatus.Active,
        };
        system.StampCreated(now, actorId);
        return system;
    }

    public void UpdateDetails(string name, string? description, DateTimeOffset now, Guid? actorId)
    {
        Name = Guard.Required(name, nameof(name), 200);
        Description = Guard.Optional(description, nameof(description), 1000);
        StampUpdated(now, actorId);
    }

    /// <param name="inUse">True while Enabled connectors or routes depend on the system (BR-CON-014).</param>
    public void Deactivate(bool inUse, DateTimeOffset now, Guid? actorId)
    {
        if (Status == ActiveStatus.Inactive)
            return;
        if (inUse)
            throw new ConflictException(
                "EXTERNAL_SYSTEM_IN_USE",
                "External system in use",
                "The external system is used by enabled connectors or routes."
            );
        Status = ActiveStatus.Inactive;
        StampUpdated(now, actorId);
    }

    public void Activate(DateTimeOffset now, Guid? actorId)
    {
        if (Status == ActiveStatus.Active)
            return;
        Status = ActiveStatus.Active;
        StampUpdated(now, actorId);
    }
}

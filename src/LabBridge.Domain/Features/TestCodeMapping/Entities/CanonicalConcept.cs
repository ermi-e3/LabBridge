using LabBridge.Domain.Common;
using LabBridge.Domain.Features.InstrumentManagement.Enums;
using LabBridge.Domain.Features.TestCodeMapping.Enums;

namespace LabBridge.Domain.Features.TestCodeMapping.Entities;

/// <summary>Organization-scoped canonical concept (test, specimen, unit...). Never deleted; only deactivated. (OrganizationId, Domain, Code) is unique.</summary>
public sealed class CanonicalConcept : AuditedAggregateRoot, IOrganizationOwned
{
    private CanonicalConcept() { }

    public Guid OrganizationId { get; private set; }
    public MappingDomain Domain { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public ActiveStatus Status { get; private set; }

    public static CanonicalConcept Create(
        Guid organizationId,
        MappingDomain domain,
        string code,
        string name,
        string? description,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        var concept = new CanonicalConcept
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
            Domain = Guard.Defined(domain, nameof(domain)),
            Code = Guard.Required(code, nameof(code), 100),
            Name = Guard.Required(name, nameof(name), 200),
            Description = Guard.Optional(description, nameof(description), 1000),
            Status = ActiveStatus.Active,
        };
        concept.StampCreated(now, actorId);
        return concept;
    }

    public void Update(string name, string? description, DateTimeOffset now, Guid? actorId)
    {
        Name = Guard.Required(name, nameof(name), 200);
        Description = Guard.Optional(description, nameof(description), 1000);
        StampUpdated(now, actorId);
    }

    public void Deactivate(DateTimeOffset now, Guid? actorId)
    {
        Status = ActiveStatus.Inactive;
        StampUpdated(now, actorId);
    }

    public void Activate(DateTimeOffset now, Guid? actorId)
    {
        Status = ActiveStatus.Active;
        StampUpdated(now, actorId);
    }
}

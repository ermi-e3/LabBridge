using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.InstrumentManagement.Enums;
using LabBridge.Domain.Features.TestCodeMapping.Enums;

namespace LabBridge.Domain.Features.TestCodeMapping.Entities;

/// <summary>
/// Container of mapping versions for one (counterparty, direction, domain). References exactly one counterparty (instrument XOR external system);
/// points at the single active (Published) version. Never deleted.
/// </summary>
public sealed class MappingSet : AuditedAggregateRoot, IOrganizationOwned
{
    private MappingSet() { }

    public Guid OrganizationId { get; private set; }
    public CounterpartyKind CounterpartyKind { get; private set; }
    public Guid? InstrumentId { get; private set; }
    public Guid? ExternalSystemId { get; private set; }
    public MappingDirection Direction { get; private set; }
    public MappingDomain Domain { get; private set; }
    public Guid? ActiveVersionId { get; private set; }
    public ActiveStatus Status { get; private set; }

    public Guid CounterpartyId => InstrumentId ?? ExternalSystemId!.Value;

    public static MappingSet ForInstrument(
        Guid organizationId,
        Guid instrumentId,
        MappingDirection direction,
        MappingDomain domain,
        DateTimeOffset now,
        Guid? actorId
    ) =>
        Create(
            organizationId,
            CounterpartyKind.Instrument,
            instrumentId,
            null,
            direction,
            domain,
            now,
            actorId
        );

    public static MappingSet ForExternalSystem(
        Guid organizationId,
        Guid externalSystemId,
        MappingDirection direction,
        MappingDomain domain,
        DateTimeOffset now,
        Guid? actorId
    ) =>
        Create(
            organizationId,
            CounterpartyKind.ExternalSystem,
            null,
            externalSystemId,
            direction,
            domain,
            now,
            actorId
        );

    private static MappingSet Create(
        Guid organizationId,
        CounterpartyKind kind,
        Guid? instrumentId,
        Guid? systemId,
        MappingDirection direction,
        MappingDomain domain,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        var set = new MappingSet
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
            CounterpartyKind = kind,
            InstrumentId = Guard.NotEmptyWhenSet(instrumentId, nameof(instrumentId)),
            ExternalSystemId = Guard.NotEmptyWhenSet(systemId, nameof(systemId)),
            Direction = Guard.Defined(direction, nameof(direction)),
            Domain = Guard.Defined(domain, nameof(domain)),
            Status = ActiveStatus.Active,
        };
        set.StampCreated(now, actorId);
        return set;
    }

    /// <summary>Points the set at a newly published version (same transaction as publication).</summary>
    public void ActivateVersion(MappingSetVersion version, DateTimeOffset now, Guid? actorId)
    {
        ArgumentNullException.ThrowIfNull(version);
        if (version.MappingSetId != Id || version.Status != VersionStatus.Published)
            throw new BusinessRuleException(
                "MAPPING_VERSION_NOT_ACTIVATABLE",
                "Version cannot be activated",
                "Only a published version of this set can become active."
            );
        ActiveVersionId = version.Id;
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

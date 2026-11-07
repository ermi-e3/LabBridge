using LabBridge.Domain.Common;
using LabBridge.Domain.Features.TestCodeMapping.Enums;

namespace LabBridge.Domain.Features.TestCodeMapping.Entities;

/// <summary>One code mapping inside a version. Direction is copied from the set. Editable only while the version is Draft.</summary>
public sealed class MappingEntry : AuditedEntity, IOrganizationOwned
{
    private MappingEntry() { }

    internal MappingEntry(
        Guid organizationId,
        Guid versionId,
        MappingDirection direction,
        string externalCode,
        string? externalCodeSystem,
        Guid conceptId,
        Guid? targetUnitConceptId,
        string? notes,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        VersionId = versionId;
        Direction = direction;
        ExternalCode = Guard.Required(externalCode, nameof(externalCode), 100);
        ExternalCodeSystem = Guard.Optional(externalCodeSystem, nameof(externalCodeSystem), 100);
        ConceptId = Guard.NotEmpty(conceptId, nameof(conceptId));
        TargetUnitConceptId = Guard.NotEmptyWhenSet(
            targetUnitConceptId,
            nameof(targetUnitConceptId)
        );
        Notes = Guard.Optional(notes, nameof(notes), 500);
        StampCreated(now, actorId);
    }

    public Guid OrganizationId { get; private set; }
    public Guid VersionId { get; private set; }
    public MappingDirection Direction { get; private set; }
    public string ExternalCode { get; private set; } = null!;
    public string? ExternalCodeSystem { get; private set; }
    public Guid ConceptId { get; private set; }
    public Guid? TargetUnitConceptId { get; private set; }
    public string? Notes { get; private set; }

    internal void Change(
        Guid conceptId,
        Guid? targetUnitConceptId,
        string? notes,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        ConceptId = Guard.NotEmpty(conceptId, nameof(conceptId));
        TargetUnitConceptId = Guard.NotEmptyWhenSet(
            targetUnitConceptId,
            nameof(targetUnitConceptId)
        );
        Notes = Guard.Optional(notes, nameof(notes), 500);
        StampUpdated(now, actorId);
    }

    internal bool Matches(string externalCode, string? externalCodeSystem) =>
        string.Equals(ExternalCode, externalCode, StringComparison.Ordinal)
        && string.Equals(
            ExternalCodeSystem ?? "",
            externalCodeSystem ?? "",
            StringComparison.Ordinal
        );
}

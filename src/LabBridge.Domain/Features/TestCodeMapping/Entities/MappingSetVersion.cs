using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.TestCodeMapping.Enums;

namespace LabBridge.Domain.Features.TestCodeMapping.Entities;

/// <summary>
/// A version of a mapping set. Entries are editable only while Draft; Publish is immutable and requires a change reason. Aggregate root of its entries.
/// ToCanonical entries are unique per (external code, code system); FromCanonical entries are unique per concept (enforced here and by partial unique indexes).
/// </summary>
public sealed class MappingSetVersion : AuditedAggregateRoot, IOrganizationOwned
{
    private readonly List<MappingEntry> _entries = [];

    private MappingSetVersion() { }

    public Guid OrganizationId { get; private set; }
    public Guid MappingSetId { get; private set; }
    public int VersionNumber { get; private set; }
    public VersionStatus Status { get; private set; }
    public UnmatchedPolicy UnmatchedPolicy { get; private set; }
    public string? ChangeReason { get; private set; }
    public Guid? CopiedFromVersionId { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }
    public Guid? PublishedBy { get; private set; }

    /// <summary>Direction of the owning set; entries copy it.</summary>
    public MappingDirection Direction { get; private set; }

    public IReadOnlyCollection<MappingEntry> Entries => _entries.AsReadOnly();

    public static MappingSetVersion CreateDraft(
        MappingSet set,
        int versionNumber,
        UnmatchedPolicy unmatchedPolicy,
        MappingSetVersion? copyFrom,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        ArgumentNullException.ThrowIfNull(set);
        if (copyFrom is not null && copyFrom.MappingSetId != set.Id)
            throw new ValidationException(
                "MAPPING_COPY_SOURCE_INVALID",
                "Invalid copy source",
                "A version can only be copied from a version of the same set."
            );
        var version = new MappingSetVersion
        {
            Id = Guid.NewGuid(),
            OrganizationId = set.OrganizationId,
            MappingSetId = set.Id,
            VersionNumber = Guard.InRange(versionNumber, 1, int.MaxValue, nameof(versionNumber)),
            Status = VersionStatus.Draft,
            UnmatchedPolicy = Guard.Defined(unmatchedPolicy, nameof(unmatchedPolicy)),
            CopiedFromVersionId = copyFrom?.Id,
            Direction = set.Direction,
        };
        version.StampCreated(now, actorId);
        if (copyFrom is not null)
            foreach (var e in copyFrom._entries)
                version._entries.Add(
                    new MappingEntry(
                        set.OrganizationId,
                        version.Id,
                        set.Direction,
                        e.ExternalCode,
                        e.ExternalCodeSystem,
                        e.ConceptId,
                        e.TargetUnitConceptId,
                        e.Notes,
                        now,
                        actorId
                    )
                );
        return version;
    }

    public void ChangeUnmatchedPolicy(UnmatchedPolicy policy, DateTimeOffset now, Guid? actorId)
    {
        EnsureDraft();
        UnmatchedPolicy = Guard.Defined(policy, nameof(policy));
        StampUpdated(now, actorId);
    }

    public MappingEntry AddEntry(
        string externalCode,
        string? externalCodeSystem,
        Guid conceptId,
        Guid? targetUnitConceptId,
        string? notes,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        EnsureDraft();
        var duplicate =
            Direction == MappingDirection.ToCanonical
                ? _entries.Any(e => e.Matches(externalCode.Trim(), externalCodeSystem?.Trim()))
                : _entries.Any(e => e.ConceptId == conceptId);
        if (duplicate)
            throw new DuplicateResourceException(
                "MAPPING_ENTRY_EXISTS",
                "Mapping entry exists",
                "An entry for this code or concept already exists in the version."
            );
        var entry = new MappingEntry(
            OrganizationId,
            Id,
            Direction,
            externalCode,
            externalCodeSystem,
            conceptId,
            targetUnitConceptId,
            notes,
            now,
            actorId
        );
        _entries.Add(entry);
        StampUpdated(now, actorId);
        return entry;
    }

    public void ChangeEntry(
        Guid entryId,
        Guid conceptId,
        Guid? targetUnitConceptId,
        string? notes,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        EnsureDraft();
        FindEntry(entryId).Change(conceptId, targetUnitConceptId, notes, now, actorId);
        StampUpdated(now, actorId);
    }

    public void RemoveEntry(Guid entryId, DateTimeOffset now, Guid? actorId)
    {
        EnsureDraft();
        _entries.Remove(FindEntry(entryId));
        StampUpdated(now, actorId);
    }

    /// <summary>Draft -> Published (immutable). A change reason is required.</summary>
    public void Publish(string changeReason, Guid actorId, DateTimeOffset now)
    {
        EnsureDraft();
        if (_entries.Count == 0)
            throw new BusinessRuleException(
                "MAPPING_VERSION_EMPTY",
                "Empty version",
                "A version without entries cannot be published."
            );
        ChangeReason = Guard.Required(changeReason, nameof(changeReason), 500);
        Status = VersionStatus.Published;
        PublishedAt = now;
        PublishedBy = actorId;
        StampUpdated(now, actorId);
    }

    public void Supersede(DateTimeOffset now)
    {
        if (Status != VersionStatus.Published)
            throw new BusinessRuleException(
                "MAPPING_VERSION_INVALID_STATE",
                "Invalid version state",
                "Only a published version can be superseded."
            );
        Status = VersionStatus.Superseded;
        StampUpdated(now, null);
    }

    /// <summary>A Draft may be discarded (its entries cascade); anything else is retained.</summary>
    public void EnsureDiscardable() => EnsureDraft();

    private MappingEntry FindEntry(Guid id) =>
        _entries.FirstOrDefault(e => e.Id == id)
        ?? throw new ResourceNotFoundException(
            "MAPPING_ENTRY_NOT_FOUND",
            "Mapping entry not found",
            "The entry does not exist in this version."
        );

    private void EnsureDraft()
    {
        if (Status != VersionStatus.Draft)
            throw new BusinessRuleException(
                "MAPPING_VERSION_NOT_DRAFT",
                "Version not editable",
                "Only a draft version can be modified."
            );
    }
}

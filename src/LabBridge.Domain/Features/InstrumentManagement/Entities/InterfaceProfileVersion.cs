using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.InstrumentManagement.Enums;

namespace LabBridge.Domain.Features.InstrumentManagement.Entities;

/// <summary>
/// A version of an interface profile (platform reference data). The definition is immutable once published (BR-CON-012).
/// Deprecated versions cannot be selected for new connectors; Withdrawn versions block enabling.
/// </summary>
public sealed class InterfaceProfileVersion : Entity
{
    private InterfaceProfileVersion() { }

    internal InterfaceProfileVersion(
        Guid profileId,
        string version,
        int definitionSchemaVersion,
        string definitionJson,
        string? releaseNotes,
        DateTimeOffset publishedAt
    )
    {
        Id = Guid.NewGuid();
        ProfileId = Guard.NotEmpty(profileId, nameof(profileId));
        Version = Guard.Required(version, nameof(version), 30);
        DefinitionSchemaVersion = Guard.InRange(
            definitionSchemaVersion,
            1,
            int.MaxValue,
            nameof(definitionSchemaVersion)
        );
        DefinitionJson = Guard.RequiredExact(definitionJson, nameof(definitionJson), int.MaxValue);
        ReleaseNotes = Guard.Optional(releaseNotes, nameof(releaseNotes), 2000);
        Status = ProfileVersionStatus.Published;
        PublishedAt = publishedAt;
    }

    public Guid ProfileId { get; private set; }

    /// <summary>Semantic version.</summary>
    public string Version { get; private set; } = null!;
    public ProfileVersionStatus Status { get; private set; }
    public int DefinitionSchemaVersion { get; private set; }
    public string DefinitionJson { get; private set; } = null!;
    public string? ReleaseNotes { get; private set; }
    public DateTimeOffset PublishedAt { get; private set; }

    public bool IsSelectableForNewConnector => Status == ProfileVersionStatus.Published;

    internal void Deprecate()
    {
        if (Status == ProfileVersionStatus.Withdrawn)
            throw new BusinessRuleException(
                "PROFILE_VERSION_INVALID_STATE",
                "Invalid profile version state",
                "A withdrawn profile version cannot be deprecated."
            );
        Status = ProfileVersionStatus.Deprecated;
    }

    internal void Withdraw() => Status = ProfileVersionStatus.Withdrawn;
}

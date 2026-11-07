using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;

namespace LabBridge.Domain.Features.InstrumentManagement.Entities;

/// <summary>Interface profile (platform reference data, no tenant). Aggregate root of its versions; versions are never deleted.</summary>
public sealed class InterfaceProfile : AggregateRoot
{
    private readonly List<InterfaceProfileVersion> _versions = [];

    private InterfaceProfile() { }

    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsSimulation { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<InterfaceProfileVersion> Versions => _versions.AsReadOnly();

    public static InterfaceProfile Create(
        string code,
        string name,
        string? description,
        bool isSimulation,
        DateTimeOffset now
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            Code = Guard.Required(code, nameof(code), 100),
            Name = Guard.Required(name, nameof(name), 200),
            Description = Guard.Optional(description, nameof(description), 1000),
            IsSimulation = isSimulation,
            CreatedAt = now,
        };

    public InterfaceProfileVersion PublishVersion(
        string version,
        int definitionSchemaVersion,
        string definitionJson,
        string? releaseNotes,
        DateTimeOffset now
    )
    {
        var trimmed = Guard.Required(version, nameof(version), 30);
        if (_versions.Any(v => v.Version == trimmed))
            throw new DuplicateResourceException(
                "PROFILE_VERSION_EXISTS",
                "Profile version exists",
                "The profile already has a version with this number."
            );
        var created = new InterfaceProfileVersion(
            Id,
            trimmed,
            definitionSchemaVersion,
            definitionJson,
            releaseNotes,
            now
        );
        _versions.Add(created);
        return created;
    }

    public void DeprecateVersion(Guid versionId) => Find(versionId).Deprecate();

    public void WithdrawVersion(Guid versionId) => Find(versionId).Withdraw();

    private InterfaceProfileVersion Find(Guid versionId) =>
        _versions.FirstOrDefault(v => v.Id == versionId)
        ?? throw new ResourceNotFoundException(
            "PROFILE_VERSION_NOT_FOUND",
            "Profile version not found",
            "The profile version does not exist."
        );
}

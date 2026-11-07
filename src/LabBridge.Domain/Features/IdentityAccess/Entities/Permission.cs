using LabBridge.Domain.Common;
using LabBridge.Domain.Features.IdentityAccess.Enums;

namespace LabBridge.Domain.Features.IdentityAccess.Entities;

/// <summary>Permission catalogue entry (platform reference data, registered by owning modules, SC-IAM-002).</summary>
public sealed class Permission : Entity
{
    private Permission() { }

    public string Code { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    /// <summary>Three-letter owning module prefix (for example "CON").</summary>
    public string OwnerModule { get; private set; } = null!;
    public PermissionScope Scope { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static Permission Create(
        string code,
        string description,
        string ownerModule,
        PermissionScope scope,
        DateTimeOffset now
    )
    {
        var module = Guard.Required(ownerModule, nameof(ownerModule), 3);
        return new Permission
        {
            Id = Guid.NewGuid(),
            Code = Guard.Required(code, nameof(code), 100),
            Description = Guard.Required(description, nameof(description), 500),
            OwnerModule = module.ToUpperInvariant(),
            Scope = Guard.Defined(scope, nameof(scope)),
            CreatedAt = now,
        };
    }
}

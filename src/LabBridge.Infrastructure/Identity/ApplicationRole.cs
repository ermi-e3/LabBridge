using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.IdentityAccess;
using LabBridge.Domain.Features.IdentityAccess.Entities;
using LabBridge.Domain.Features.IdentityAccess.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace LabBridge.Infrastructure.Identity;

/// <summary>
/// System role as an ASP.NET Core Identity role (platform reference data, changed only through released migrations, BR-IAM-011).
/// ARCHITECTURE: replaces the Domain "Role" aggregate. Identity's Name holds the stable role code (for example
/// "OrgAdmin", see SystemRoleCodes); DisplayName holds the former human-readable Name.
/// </summary>
public sealed class ApplicationRole : IdentityRole<Guid>
{
    private readonly List<RolePermission> _permissions = [];

    private ApplicationRole() { }

    /// <summary>Stable code. Alias of Identity's Name so existing rules that speak of "Code" keep working; not mapped.</summary>
    public string Code => Name ?? string.Empty;

    public string DisplayName { get; private set; } = null!;
    public string? Description { get; private set; }
    public RoleScope Scope { get; private set; }
    public bool IsSystem { get; private set; }

    /// <summary>Additive roles (for example RawPayloadReader) may only be combined with a primary role (BR-IAM-009).</summary>
    public bool IsAdditive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();

    /// <remarks>
    /// NormalizedName and ConcurrencyStamp are filled by RoleManager.CreateAsync. When seeding through a migration they
    /// must be set explicitly (NormalizedName = code upper-cased with the invariant culture).
    /// </remarks>
    public static ApplicationRole CreateSystemRole(
        string code,
        string displayName,
        string? description,
        RoleScope scope,
        bool isAdditive,
        DateTimeOffset now
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = Guard.Required(code, nameof(code), 50),
            DisplayName = Guard.Required(displayName, nameof(displayName), 100),
            Description = Guard.Optional(description, nameof(description), 500),
            Scope = Guard.Defined(scope, nameof(scope)),
            IsSystem = true,
            IsAdditive = isAdditive,
            CreatedAt = now,
        };

    public bool Grants(Guid permissionId) => _permissions.Any(p => p.PermissionId == permissionId);

    /// <remarks>Requires Permissions to be loaded (Include) by the caller.</remarks>
    public RolePermission GrantPermission(Permission permission)
    {
        var grant = RolePermission.Grant(
            Id,
            Scope,
            permission,
            _permissions.Select(p => p.PermissionId)
        );
        _permissions.Add(grant);
        return grant;
    }

    public void RevokePermission(Guid permissionId)
    {
        var existing =
            _permissions.FirstOrDefault(p => p.PermissionId == permissionId)
            ?? throw new ResourceNotFoundException(
                "ROLE_PERMISSION_NOT_FOUND",
                "Role permission not found",
                "The role does not grant the specified permission."
            );
        _permissions.Remove(existing);
    }

    /// <summary>Framework-independent view handed to Domain policies.</summary>
    public RoleSnapshot ToSnapshot() => new(Id, Code, Scope, IsAdditive);
}
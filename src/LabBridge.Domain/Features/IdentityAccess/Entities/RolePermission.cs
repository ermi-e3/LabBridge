using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.IdentityAccess.Enums;

namespace LabBridge.Domain.Features.IdentityAccess.Entities;

/// <summary>Association of a role with a permission (composite key RoleId + PermissionId). Owned by the Role aggregate.</summary>
//public sealed class RolePermission
//{
//    private RolePermission() { }

//    internal RolePermission(Guid roleId, Guid permissionId)
//    {
//        RoleId = Guard.NotEmpty(roleId, nameof(roleId));
//        PermissionId = Guard.NotEmpty(permissionId, nameof(permissionId));
//    }

//    public Guid RoleId { get; private set; }
//    public Guid PermissionId { get; private set; }
//}



/// <summary>
/// Association of a role with a permission (composite key RoleId + PermissionId).
/// ARCHITECTURE: the role is referenced by identifier only. The role itself is an ASP.NET Core Identity entity
/// (ApplicationRole) in Infrastructure, so the Domain never depends on it.
/// </summary>
public sealed class RolePermission
{
    private RolePermission() { }

    private RolePermission(Guid roleId, Guid permissionId)
    {
        RoleId = Guard.NotEmpty(roleId, nameof(roleId));
        PermissionId = Guard.NotEmpty(permissionId, nameof(permissionId));
    }

    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }

    // CHANGE: the grant rules (uniqueness, scope compatibility) moved here from the former Role aggregate, because
    // the internal constructor was only reachable from Role, which is no longer in the Domain assembly.
    public static RolePermission Grant(
        Guid roleId,
        RoleScope roleScope,
        Permission permission,
        IEnumerable<Guid> alreadyGrantedPermissionIds
    )
    {
        ArgumentNullException.ThrowIfNull(permission);
        ArgumentNullException.ThrowIfNull(alreadyGrantedPermissionIds);
        Guard.Defined(roleScope, nameof(roleScope));

        if (alreadyGrantedPermissionIds.Contains(permission.Id))
            throw new DuplicateResourceException(
                "ROLE_PERMISSION_ALREADY_GRANTED",
                "Permission already granted",
                "The role already grants this permission."
            );
        if (!IsScopeCompatible(permission.Scope, roleScope))
            throw new BusinessRuleException(
                "ROLE_PERMISSION_SCOPE_MISMATCH",
                "Permission scope mismatch",
                "The permission scope is not compatible with the role scope."
            );
        return new RolePermission(roleId, permission.Id);
    }

    // CHANGE: explicit mapping replaces the former (int) cast comparison, which only worked while PermissionScope
    // and RoleScope happened to share numeric values.
    private static bool IsScopeCompatible(PermissionScope permissionScope, RoleScope roleScope) =>
        permissionScope switch
        {
            PermissionScope.Both => true,
            PermissionScope.Platform => roleScope == RoleScope.Platform,
            PermissionScope.Organization => roleScope == RoleScope.Organization,
            _ => false,
        };
}
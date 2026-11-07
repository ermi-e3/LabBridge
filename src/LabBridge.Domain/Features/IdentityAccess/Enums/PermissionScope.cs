namespace LabBridge.Domain.Features.IdentityAccess.Enums;

/// <summary>Scope in which a permission may be exercised.</summary>
public enum PermissionScope
{
    Platform = 0,
    Organization = 1,
    Both = 2,
}

namespace LabBridge.Domain.Common;

/// <summary>
/// Marker for tenant-owned entities (SC-ORG-003). OrganizationId is never null for these entities,
/// is set from the tenant context at creation and never changes.
/// </summary>
public interface IOrganizationOwned
{
    Guid OrganizationId { get; }
}

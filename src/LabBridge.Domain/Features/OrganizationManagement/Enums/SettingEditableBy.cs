namespace LabBridge.Domain.Features.OrganizationManagement.Enums;

/// <summary>Which scope may change a setting (BR-ORG-013).</summary>
public enum SettingEditableBy
{
    Platform = 0,
    Organization = 1,
    Both = 2,
}

namespace LabBridge.Domain.Features.OrganizationManagement.Enums;

/// <summary>Organization lifecycle status (Stage 1 / Module 2 §4.0). Deactivated is terminal in the MVP (BR-ORG-003).</summary>
public enum OrganizationStatus
{
    Provisioning = 0,
    Active = 1,
    Suspended = 2,
    Deactivated = 3,
}

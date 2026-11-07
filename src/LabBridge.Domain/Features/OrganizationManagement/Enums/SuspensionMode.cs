namespace LabBridge.Domain.Features.OrganizationManagement.Enums;

/// <summary>How a suspended organization treats inbound traffic (BR-ORG-008, DEC-034 Proposed).</summary>
public enum SuspensionMode
{
    HoldAndPreserve = 0,
    BlockAll = 1,
}

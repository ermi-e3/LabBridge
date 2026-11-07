namespace LabBridge.Domain.Features.DeliveryManagement.Enums;

/// <summary>Why a Pending/RetryScheduled item is held. ASSUMPTION A-DLV-02: derived from the dispatch preconditions of FR-DLV-003 (OrganizationPolicy is quoted in the FRS).</summary>
public enum DeliveryHoldReason
{
    OrganizationPolicy = 0,
    RouteNotEffective = 1,
    ConnectorNotEnabled = 2,
    CircuitOpen = 3
}

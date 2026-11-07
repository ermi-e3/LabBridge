namespace LabBridge.Domain.Features.DeliveryManagement.Enums;

/// <summary>Reconciliation case status. One Open case per attempt.</summary>
public enum ReconciliationCaseStatus
{
    Open = 0,
    Resolved = 1,
    Escalated = 2,
}

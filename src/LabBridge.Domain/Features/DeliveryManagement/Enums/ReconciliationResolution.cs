namespace LabBridge.Domain.Features.DeliveryManagement.Enums;

/// <summary>How an uncertain delivery was resolved.</summary>
public enum ReconciliationResolution
{
    ConfirmedDelivered = 0,
    ConfirmedNotDelivered = 1,
    Resent = 2,
    MarkedFailed = 3,
    LateAcknowledgment = 4,
    StatusQuery = 5,
}

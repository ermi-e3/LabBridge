namespace LabBridge.Domain.Features.DeliveryManagement.Enums;

/// <summary>Delivery item state (Stage 1 sections 8.4/10.6; Module 8 section 4.0).</summary>
public enum DeliveryState
{
    Pending = 0,
    InProgress = 1,
    Succeeded = 2,
    RetryScheduled = 3,
    FailedPermanent = 4,
    Uncertain = 5,
    Quarantined = 6,
}

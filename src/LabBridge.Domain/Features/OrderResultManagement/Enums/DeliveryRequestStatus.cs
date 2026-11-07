namespace LabBridge.Domain.Features.OrderResultManagement.Enums;

/// <summary>Delivery request lifecycle (consumed by Module 8).</summary>
public enum DeliveryRequestStatus
{
    Pending = 0,
    Claimed = 1,
    Completed = 2,
    Cancelled = 3,
}

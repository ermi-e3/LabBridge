namespace LabBridge.Domain.Features.OrderResultManagement.Enums;

/// <summary>Order lifecycle (Module 7 section 4.0). Module 7 is the single writer (BR-WFL-009). Cancelled/Superseded are later-phase.</summary>
public enum OrderStatus
{
    Received = 0,
    Validated = 1,
    Rejected = 2,
    MappingFailed = 3,
    Mapped = 4,
    ReadyForDelivery = 5,
    Delivering = 6,
    Sent = 7,
    Acknowledged = 8,
    Accepted = 9,
    DeliveryFailed = 10,
    DeliveryUncertain = 11,
    Quarantined = 12,
}

namespace LabBridge.Domain.Features.OrderResultManagement.Enums;

/// <summary>Result lifecycle (Module 7 section 4.0). Superseded is later-phase.</summary>
public enum ResultStatus
{
    Received = 0,
    Unmatched = 1,
    Validated = 2,
    Rejected = 3,
    Normalized = 4,
    MappingFailed = 5,
    Mapped = 6,
    ReadyForDelivery = 7,
    Delivering = 8,
    Sent = 9,
    Acknowledged = 10,
    Accepted = 11,
    DestinationRejected = 12,
    DeliveryFailed = 13,
    DeliveryUncertain = 14,
    Quarantined = 15,
}

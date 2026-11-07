namespace LabBridge.Domain.Features.DeliveryManagement.Enums;

/// <summary>Whether an inbound acknowledgment was matched to an attempt.</summary>
public enum AckMatchStatus
{
    Unmatched = 0,
    Matched = 1,
    Ignored = 2,
}

namespace LabBridge.Domain.Features.DeliveryManagement.Enums;

/// <summary>Application-level acknowledgment outcome.</summary>
public enum AckOutcome
{
    NotApplicable = 0,
    Pending = 1,
    Positive = 2,
    Negative = 3,
    NotReceived = 4
}

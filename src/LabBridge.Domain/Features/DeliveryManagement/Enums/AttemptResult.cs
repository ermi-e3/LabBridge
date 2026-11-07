namespace LabBridge.Domain.Features.DeliveryManagement.Enums;

/// <summary>Derived overall result of an attempt.</summary>
public enum AttemptResult
{
    InProgress = 0,
    Succeeded = 1,
    FailedRetryable = 2,
    FailedPermanent = 3,
    Uncertain = 4
}

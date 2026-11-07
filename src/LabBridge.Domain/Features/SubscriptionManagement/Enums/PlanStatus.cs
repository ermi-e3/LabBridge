namespace LabBridge.Domain.Features.SubscriptionManagement.Enums;

/// <summary>Plan version lifecycle. Published versions are immutable (BR-SUB-002).</summary>
public enum PlanStatus
{
    Draft = 0,
    Published = 1,
    Retired = 2,
}

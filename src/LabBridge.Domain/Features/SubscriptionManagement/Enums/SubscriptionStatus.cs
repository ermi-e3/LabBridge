namespace LabBridge.Domain.Features.SubscriptionManagement.Enums;

/// <summary>Commercial subscription status; independent of organization status (BR-SUB-004).</summary>
public enum SubscriptionStatus
{
    Trial = 0,
    Active = 1,
    Suspended = 2,
    Cancelled = 3,
}

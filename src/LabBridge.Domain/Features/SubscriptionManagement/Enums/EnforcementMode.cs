namespace LabBridge.Domain.Features.SubscriptionManagement.Enums;

/// <summary>How a limit is enforced. Message-volume limits are never Block (BR-SUB-001).</summary>
public enum EnforcementMode
{
    Observe = 0,
    Warn = 1,
    Block = 2,
}

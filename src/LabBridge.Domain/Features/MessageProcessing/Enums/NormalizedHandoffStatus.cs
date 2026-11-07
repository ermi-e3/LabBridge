namespace LabBridge.Domain.Features.MessageProcessing.Enums;

/// <summary>Normalized hand-off lifecycle (no Held state).</summary>
public enum NormalizedHandoffStatus
{
    Pending = 0,
    Claimed = 1,
    Completed = 2,
    Cancelled = 3,
}

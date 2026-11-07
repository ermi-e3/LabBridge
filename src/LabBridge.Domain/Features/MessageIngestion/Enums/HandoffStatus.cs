namespace LabBridge.Domain.Features.MessageIngestion.Enums;

/// <summary>Processing hand-off lifecycle: Pending -> Claimed -> Completed; Claimed -> Pending on release or lease expiry; Pending <-> Held; non-Completed -> Cancelled.</summary>
public enum HandoffStatus
{
    Pending = 0,
    Held = 1,
    Claimed = 2,
    Completed = 3,
    Cancelled = 4,
}

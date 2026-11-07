namespace LabBridge.Domain.Features.MessageProcessing.Enums;

/// <summary>Terminal outcome of a completed run.</summary>
public enum ProcessingOutcome
{
    Normalized = 0,
    Unsupported = 1,
    Invalid = 2,
    Failed = 3,
}

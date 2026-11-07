namespace LabBridge.Domain.Features.MessageProcessing.Enums;

/// <summary>What caused a processing run.</summary>
public enum ProcessingTrigger
{
    Handoff = 0,
    AutoRetry = 1,
    ManualReprocess = 2,
}

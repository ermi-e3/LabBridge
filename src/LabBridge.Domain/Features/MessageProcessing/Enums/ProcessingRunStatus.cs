namespace LabBridge.Domain.Features.MessageProcessing.Enums;

/// <summary>Processing run lifecycle. At most one open run (Scheduled/InProgress) per message.</summary>
public enum ProcessingRunStatus
{
    Scheduled = 0,
    InProgress = 1,
    Completed = 2,
    Abandoned = 3,
}

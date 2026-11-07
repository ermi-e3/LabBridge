namespace LabBridge.Domain.Features.MessageProcessing.Enums;

/// <summary>Operator decision on a failed or invalid message.</summary>
public enum ReviewDecision
{
    Reprocess = 0,
    Dismiss = 1,
}

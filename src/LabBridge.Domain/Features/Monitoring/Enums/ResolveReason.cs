namespace LabBridge.Domain.Features.Monitoring.Enums;

/// <summary>Why an alert was resolved.</summary>
public enum ResolveReason
{
    ConditionCleared = 0,
    Manual = 1,
    Superseded = 2,
}

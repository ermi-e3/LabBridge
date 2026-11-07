namespace LabBridge.Domain.Features.Monitoring.Enums;

/// <summary>How an alert rule fires.</summary>
public enum RuleKind
{
    EventTriggered = 0,
    Threshold = 1,
}

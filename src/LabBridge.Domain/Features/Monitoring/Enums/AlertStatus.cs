namespace LabBridge.Domain.Features.Monitoring.Enums;

/// <summary>Alert lifecycle: Open -> Acknowledged -> Resolved (Open may resolve directly).</summary>
public enum AlertStatus
{
    Open = 0,
    Acknowledged = 1,
    Resolved = 2,
}

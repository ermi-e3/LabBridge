namespace LabBridge.Domain.Features.InstrumentManagement.Enums;

/// <summary>System-computed connector health (BR-CON-015).</summary>
public enum ConnectorHealthStatus
{
    Unknown = 0,
    Healthy = 1,
    Degraded = 2,
    Unavailable = 3,
    Disabled = 4,
}

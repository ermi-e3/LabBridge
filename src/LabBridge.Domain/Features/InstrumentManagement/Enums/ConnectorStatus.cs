namespace LabBridge.Domain.Features.InstrumentManagement.Enums;

/// <summary>Connector configuration lifecycle: Draft -> Enabled <-> Disabled.</summary>
public enum ConnectorStatus
{
    Draft = 0,
    Enabled = 1,
    Disabled = 2,
}

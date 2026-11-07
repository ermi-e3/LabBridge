namespace LabBridge.Domain.Features.InstrumentManagement.Enums;

/// <summary>Direction relative to LabBridge: Inbound = LabBridge receives, Outbound = LabBridge sends, Bidirectional = both (BR-CON-002).</summary>
public enum ConnectorDirection
{
    Inbound = 0,
    Outbound = 1,
    Bidirectional = 2,
}

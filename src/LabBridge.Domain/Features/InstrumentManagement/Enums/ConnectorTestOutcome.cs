namespace LabBridge.Domain.Features.InstrumentManagement.Enums;

/// <summary>Outcome of a connector connection test.</summary>
public enum ConnectorTestOutcome
{
    Failed = 0,
    Passed = 1,
    TimedOut = 2,
    NotSupported = 3,
}

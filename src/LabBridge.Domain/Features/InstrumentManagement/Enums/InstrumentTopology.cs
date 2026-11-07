namespace LabBridge.Domain.Features.InstrumentManagement.Enums;

/// <summary>Whether order source and result destination are the same system (BR-CON-021).</summary>
public enum InstrumentTopology
{
    SameSystem = 0,
    DifferentSystems = 1,
}

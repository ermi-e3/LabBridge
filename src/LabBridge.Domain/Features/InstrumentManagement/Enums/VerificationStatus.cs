namespace LabBridge.Domain.Features.InstrumentManagement.Enums;

/// <summary>Highest verification level reached for the instrument's current profile version (BR-CON-005).</summary>
public enum VerificationStatus
{
    Unverified = 0,
    SimulatorVerified = 1,
    HardwareVerified = 2,
}

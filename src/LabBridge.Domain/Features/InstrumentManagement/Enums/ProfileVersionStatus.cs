namespace LabBridge.Domain.Features.InstrumentManagement.Enums;

/// <summary>Interface profile version lifecycle. Published versions are immutable (BR-CON-012).</summary>
public enum ProfileVersionStatus
{
    Unknown = 0,
    Published = 1,
    Deprecated = 2,
    Withdrawn = 3,
}

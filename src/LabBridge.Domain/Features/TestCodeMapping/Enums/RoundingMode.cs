namespace LabBridge.Domain.Features.TestCodeMapping.Enums;

/// <summary>Rounding mode of a conversion rule. ASSUMPTION A-MAP-02: values are not enumerated in the FRS (varchar 10).</summary>
public enum RoundingMode
{
    HalfUp = 0,
    HalfEven = 1,
    Down = 2,
}

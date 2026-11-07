namespace LabBridge.Domain.Features.TestCodeMapping.Enums;

/// <summary>Domain of a canonical concept / mapping set. ASSUMPTION A-MAP-01: the FRS fixes the column (varchar 15) but the examples used by rules are Test, Specimen and Unit; stored as text so adding values needs no schema change.</summary>
public enum MappingDomain
{
    Test = 0,
    Specimen = 1,
    Unit = 2,
}

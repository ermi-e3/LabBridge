namespace LabBridge.Domain.Features.TestCodeMapping.Enums;

/// <summary>Kind of mapping gap (also used as the failure reason of a failed resolution).</summary>
public enum MappingGapKind
{
    MappingNotConfigured = 0,
    NoSourceMapping = 1,
    NoTargetMapping = 2,
    UnitNotMapped = 3,
    UnitMissing = 4,
    NoConversionRule = 5
}

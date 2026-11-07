namespace LabBridge.Domain.Features.TestCodeMapping.Enums;

/// <summary>Mapping gap lifecycle. Open/Ignored gaps are unique per key.</summary>
public enum GapStatus
{
    Open = 0,
    Resolved = 1,
    Ignored = 2,
}

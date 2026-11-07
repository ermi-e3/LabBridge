namespace LabBridge.Domain.Features.TestCodeMapping.Enums;

/// <summary>Mapping set version lifecycle: Draft -> Published -> Superseded. One Draft and one Published per set.</summary>
public enum VersionStatus
{
    Draft = 0,
    Published = 1,
    Superseded = 2,
}

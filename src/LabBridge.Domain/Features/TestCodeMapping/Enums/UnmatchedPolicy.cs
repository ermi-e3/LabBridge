namespace LabBridge.Domain.Features.TestCodeMapping.Enums;

/// <summary>Behaviour when no mapping exists for a code.</summary>
public enum UnmatchedPolicy
{
    Fail = 0,
    PassThrough = 1,
}

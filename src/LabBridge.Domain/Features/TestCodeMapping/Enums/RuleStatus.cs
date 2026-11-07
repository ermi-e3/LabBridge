namespace LabBridge.Domain.Features.TestCodeMapping.Enums;

/// <summary>Unit conversion rule lifecycle: Draft -> Approved -> Retired. Only Approved rules are applied.</summary>
public enum RuleStatus
{
    Draft = 0,
    Approved = 1,
    Retired = 2,
}

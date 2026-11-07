namespace LabBridge.Domain.Features.IdentityAccess;

/// <summary>Stable codes of system roles that are referenced by documented business rules (BR-IAM-006, 008, 009, 013).</summary>
public static class SystemRoleCodes
{
    public const string PlatformAdmin = "PlatformAdmin";
    public const string OrgAdmin = "OrgAdmin";
    public const string Auditor = "Auditor";
    public const string RawPayloadReader = "RawPayloadReader";
}

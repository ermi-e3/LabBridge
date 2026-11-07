namespace LabBridge.Domain.Features.Audit.Enums;

/// <summary>Audit stream scope. The platform stream has no organization (documented tenant exception).</summary>
public enum AuditScope
{
    Organization = 0,
    Platform = 1,
}

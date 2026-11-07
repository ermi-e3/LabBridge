namespace LabBridge.Domain.Features.Audit.Enums;

/// <summary>Kind of actor of an audit event.</summary>
public enum AuditActorType
{
    User = 0,
    System = 1,
    Connector = 2,
    Platform = 3,
}

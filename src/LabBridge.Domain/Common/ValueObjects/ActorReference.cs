namespace LabBridge.Domain.Common.ValueObjects;

/// <summary>Kind of actor that caused a change (used by history and audit records).</summary>
public enum ActorType
{
    System = 0,
    User = 1,
    PlatformOperator = 2
}

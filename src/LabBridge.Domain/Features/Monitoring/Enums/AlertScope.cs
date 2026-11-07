namespace LabBridge.Domain.Features.Monitoring.Enums;

/// <summary>Alert/rule scope. Platform alerts have no organization (documented tenant exception).</summary>
public enum AlertScope
{
    Organization = 0,
    Platform = 1,
}

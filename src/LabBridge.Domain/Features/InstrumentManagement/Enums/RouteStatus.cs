namespace LabBridge.Domain.Features.InstrumentManagement.Enums;

/// <summary>Route status. Routes are created Disabled; effective only if both connectors are Enabled.</summary>
public enum RouteStatus
{
    Disabled = 0,
    Enabled = 1,
}

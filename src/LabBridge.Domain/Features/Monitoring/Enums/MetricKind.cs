namespace LabBridge.Domain.Features.Monitoring.Enums;

/// <summary>Metric sample kind. ASSUMPTION A-MON-01: the FRS stores a kind varchar; Counter accumulates, Gauge replaces.</summary>
public enum MetricKind
{
    Counter = 0,
    Gauge = 1,
}

using LabBridge.Domain.Common;
using LabBridge.Domain.Features.Monitoring.Enums;

namespace LabBridge.Domain.Features.Monitoring.Entities;

/// <summary>Append-only history of alert state changes (same retention as the alert). Not tenant-keyed: platform alerts have no organization.</summary>
public sealed class AlertStateHistory : Entity
{
    private AlertStateHistory() { }

    internal AlertStateHistory(
        Guid alertId,
        AlertStatus? from,
        AlertStatus to,
        string? reason,
        Guid? actorId,
        DateTimeOffset changedAt
    )
    {
        Id = Guid.NewGuid();
        AlertId = alertId;
        FromStatus = from;
        ToStatus = to;
        Reason = Guard.Optional(reason, nameof(reason), 500);
        ActorId = actorId;
        ChangedAt = changedAt;
    }

    public Guid AlertId { get; private set; }
    public AlertStatus? FromStatus { get; private set; }
    public AlertStatus ToStatus { get; private set; }
    public string? Reason { get; private set; }
    public Guid? ActorId { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
}

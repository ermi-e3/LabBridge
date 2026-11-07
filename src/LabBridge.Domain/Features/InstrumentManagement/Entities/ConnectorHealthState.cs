using LabBridge.Domain.Common;
using LabBridge.Domain.Features.InstrumentManagement.Enums;

namespace LabBridge.Domain.Features.InstrumentManagement.Entities;

/// <summary>
/// Runtime-computed health of a connector (1:1 with Connector, shares its identifier). Separate aggregate because it is written
/// frequently by a single runtime owner (lease), last-write-wins with monotonic UpdatedAt (BR-CON-015). No user write path.
/// </summary>
public sealed class ConnectorHealthState : AggregateRoot, IOrganizationOwned
{
    private ConnectorHealthState() { }

    public Guid OrganizationId { get; private set; }
    public ConnectorHealthStatus Status { get; private set; }
    public DateTimeOffset? LastCheckedAt { get; private set; }
    public DateTimeOffset? LastSuccessAt { get; private set; }
    public DateTimeOffset? LastFailureAt { get; private set; }
    public int ConsecutiveFailures { get; private set; }
    public string? LastErrorCode { get; private set; }

    /// <summary>Sanitized: no secrets, no patient data.</summary>
    public string? LastErrorSummary { get; private set; }
    public int? AppliedConfigVersion { get; private set; }
    public DateTimeOffset StatusChangedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public Guid ConnectorId => Id;

    public static ConnectorHealthState Initialize(Connector connector, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(connector);
        return new ConnectorHealthState
        {
            Id = connector.Id,
            OrganizationId = connector.OrganizationId,
            Status =
                connector.Status == ConnectorStatus.Disabled
                    ? ConnectorHealthStatus.Disabled
                    : ConnectorHealthStatus.Unknown,
            StatusChangedAt = now,
            UpdatedAt = now,
        };
    }

    public void RecordSuccess(int appliedConfigVersion, DateTimeOffset now)
    {
        if (now < UpdatedAt)
            return; // monotonic: ignore stale writes
        LastCheckedAt = now;
        LastSuccessAt = now;
        ConsecutiveFailures = 0;
        AppliedConfigVersion = appliedConfigVersion;
        SetStatus(ConnectorHealthStatus.Healthy, now);
    }

    /// <param name="unavailableAfterFailures">Consecutive failures after which the connector is Unavailable (otherwise Degraded).</param>
    public void RecordFailure(
        string errorCode,
        string? sanitizedSummary,
        int unavailableAfterFailures,
        DateTimeOffset now
    )
    {
        if (now < UpdatedAt)
            return;
        Guard.InRange(unavailableAfterFailures, 1, 100000, nameof(unavailableAfterFailures));
        LastCheckedAt = now;
        LastFailureAt = now;
        ConsecutiveFailures++;
        LastErrorCode = Guard.Required(errorCode, nameof(errorCode), 50);
        LastErrorSummary = Guard.Optional(sanitizedSummary, nameof(sanitizedSummary), 500);
        SetStatus(
            ConsecutiveFailures >= unavailableAfterFailures
                ? ConnectorHealthStatus.Unavailable
                : ConnectorHealthStatus.Degraded,
            now
        );
    }

    public void MarkDisabled(DateTimeOffset now) => SetStatus(ConnectorHealthStatus.Disabled, now);

    public void ResetToUnknown(DateTimeOffset now)
    {
        ConsecutiveFailures = 0;
        SetStatus(ConnectorHealthStatus.Unknown, now);
    }

    private void SetStatus(ConnectorHealthStatus status, DateTimeOffset now)
    {
        if (Status != status)
        {
            Status = status;
            StatusChangedAt = now;
        }
        if (now > UpdatedAt)       // never move backwards
            UpdatedAt = now;
    }
}

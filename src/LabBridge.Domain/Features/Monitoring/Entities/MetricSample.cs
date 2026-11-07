using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.Monitoring.Enums;

namespace LabBridge.Domain.Features.Monitoring.Entities;

/// <summary>
/// Aggregated metric value per (organization, metric, dimensions, time bucket); written by atomic upsert. Dimensions are codes/identifiers only,
/// never content. Counter samples accumulate, gauge samples replace.
/// </summary>
public sealed class MetricSample : Entity, IOrganizationOwned
{
    private MetricSample() { }

    public Guid OrganizationId { get; private set; }
    public string MetricName { get; private set; } = null!;
    public string DimensionsKey { get; private set; } = null!;
    public DateTimeOffset BucketStart { get; private set; }
    public MetricKind Kind { get; private set; }
    public decimal Value { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static MetricSample Start(
        Guid organizationId,
        string metricName,
        string dimensionsKey,
        DateTimeOffset bucketStart,
        MetricKind kind,
        decimal value,
        DateTimeOffset now
    )
    {
        kind = Guard.Defined(kind, nameof(kind));

        if (kind == MetricKind.Counter && value < 0)
        {
            throw new ValidationException(
                "METRIC_COUNTER_NEGATIVE",
                "Invalid counter value",
                "A counter can only increase.");
        }

        return new()
            {
                Id = Guid.NewGuid(),
                OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
                MetricName = Guard.Required(metricName, nameof(metricName), 100),
                DimensionsKey = Guard.RequiredExact(dimensionsKey, nameof(dimensionsKey), 300),
                BucketStart = bucketStart,
                Kind = Guard.Defined(kind, nameof(kind)),
                Value = value,
                UpdatedAt = now,
            };
    }
    public void Record(decimal value, DateTimeOffset now)
    {
        if (Kind == MetricKind.Counter && value < 0)
            throw new ValidationException("METRIC_COUNTER_NEGATIVE", "Invalid counter value",
                "A counter can only increase.");
        Value = Kind == MetricKind.Counter ? Value + value : value;
        UpdatedAt = now;
    }
}

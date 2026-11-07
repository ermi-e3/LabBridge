using LabBridge.Domain.Common;

namespace LabBridge.Domain.Features.SubscriptionManagement.Entities;

/// <summary>Metered usage per (organization, period, metric key), written by atomic upsert; signal-id deduplication happens at the metering boundary (BR-SUB-005).</summary>
public sealed class UsageCounter : Entity, IOrganizationOwned
{
    private UsageCounter() { }

    public Guid OrganizationId { get; private set; }

    /// <summary>Billing period identifier, for example "2026-10".</summary>
    public string Period { get; private set; } = null!;
    public string MetricKey { get; private set; } = null!;
    public decimal Value { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static UsageCounter Start(
        Guid organizationId,
        string period,
        string metricKey,
        decimal initialValue,
        DateTimeOffset now
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
            Period = Guard.Required(period, nameof(period), 20),
            MetricKey = Guard.Required(metricKey, nameof(metricKey), 100),
            Value = initialValue,
            UpdatedAt = now,
        };

    public void Add(decimal delta, DateTimeOffset now)
    {
        Value += delta;
        UpdatedAt = now;
    }

    public void Set(decimal value, DateTimeOffset now)
    {
        Value = value;
        UpdatedAt = now;
    }
}

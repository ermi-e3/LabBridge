using LabBridge.Domain.Common;
using LabBridge.Domain.Features.SubscriptionManagement.Enums;

namespace LabBridge.Domain.Features.SubscriptionManagement.Entities;

/// <summary>Definition of an entitlement limit (platform reference data; never deleted). Natural key: limit key.</summary>
public sealed class LimitDefinition
{
    private LimitDefinition() { }

    public string LimitKey { get; private set; } = null!;
    public LimitKind Kind { get; private set; }
    public string? Unit { get; private set; }
    public string? MetricSource { get; private set; }
    public string Description { get; private set; } = null!;

    /// <summary>Message-volume limits may only be Observe or Warn (BR-SUB-001).</summary>
    public bool IsMessageVolume { get; private set; }
    public bool IsActive { get; private set; }

    public static LimitDefinition Define(
        string limitKey,
        LimitKind kind,
        string? unit,
        string? metricSource,
        string description,
        bool isMessageVolume
    ) =>
        new()
        {
            LimitKey = Guard.Required(limitKey, nameof(limitKey), 100),
            Kind = Guard.Defined(kind, nameof(kind)),
            Unit = Guard.Optional(unit, nameof(unit), 50),
            MetricSource = Guard.Optional(metricSource, nameof(metricSource), 100),
            Description = Guard.Required(description, nameof(description), 500),
            IsMessageVolume = isMessageVolume,
            IsActive = true,
        };

    public void Deactivate() => IsActive = false;
}

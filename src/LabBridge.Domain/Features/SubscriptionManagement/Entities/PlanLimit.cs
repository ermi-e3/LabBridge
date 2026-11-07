using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.SubscriptionManagement.Enums;

namespace LabBridge.Domain.Features.SubscriptionManagement.Entities;

/// <summary>A limit within a plan version: numeric value (Quantity) or feature flag (Feature) with its enforcement mode. Unique per (plan, limit key).</summary>
public sealed class PlanLimit : Entity
{
    private PlanLimit() { }

    internal PlanLimit(
        Guid planId,
        LimitDefinition definition,
        decimal? limitValue,
        bool featureEnabled,
        EnforcementMode mode
    )
    {
        if (definition.IsMessageVolume && mode == EnforcementMode.Block)
            throw new BusinessRuleException(
                "PLAN_LIMIT_BLOCK_FORBIDDEN",
                "Blocking not allowed",
                "Message-volume limits may only be Observe or Warn."
            );
        if (definition.Kind == LimitKind.Quantity && (limitValue is null or < 0))
            throw new ValidationException(
                "PLAN_LIMIT_VALUE_INVALID",
                "Invalid limit value",
                "A quantity limit requires a non-negative value."
            );
        Id = Guid.NewGuid();
        PlanId = planId;
        LimitKey = definition.LimitKey;
        Kind = definition.Kind;
        IsMessageVolume = definition.IsMessageVolume;
        LimitValue = definition.Kind == LimitKind.Quantity ? limitValue : null;
        FeatureEnabled = definition.Kind == LimitKind.Feature && featureEnabled;
        EnforcementMode = Guard.Defined(mode, nameof(mode));
    }

    public Guid PlanId { get; private set; }
    public string LimitKey { get; private set; } = null!;
    public LimitKind Kind { get; private set; }               // copied from the definition (the foreign key keeps it consistent)
    public bool IsMessageVolume { get; private set; }        // copied from the definition
    public decimal? LimitValue { get; private set; }
    public bool FeatureEnabled { get; private set; }
    public EnforcementMode EnforcementMode { get; private set; }
}

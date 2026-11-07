using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.SubscriptionManagement.Enums;

namespace LabBridge.Domain.Features.SubscriptionManagement.Entities;

/// <summary>Versioned plan (platform data; no plans are shipped, BR-SUB-002). Limits are editable only in Draft; Published is immutable; Retired stops new assignments.</summary>
public sealed class Plan : AggregateRoot
{
    private readonly List<PlanLimit> _limits = [];

    private Plan() { }

    public string Code { get; private set; } = null!;
    public int Version { get; private set; }
    public string Name { get; private set; } = null!;
    public PlanStatus Status { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }

    public IReadOnlyCollection<PlanLimit> Limits => _limits.AsReadOnly();

    public static Plan CreateDraft(
        string code,
        int version,
        string name,
        DateTimeOffset now,
        Guid? actorId
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            Code = Guard.Required(code, nameof(code), 50),
            Version = Guard.InRange(version, 1, int.MaxValue, nameof(version)),
            Name = Guard.Required(name, nameof(name), 200),
            Status = PlanStatus.Draft,
            CreatedAt = now,
            CreatedBy = actorId,
        };

    public PlanLimit AddLimit(
        LimitDefinition definition,
        decimal? limitValue,
        bool featureEnabled,
        EnforcementMode mode
    )
    {
        ArgumentNullException.ThrowIfNull(definition);
        EnsureDraft();
        if (!definition.IsActive)
            throw new BusinessRuleException(
                "PLAN_LIMIT_DEFINITION_INACTIVE",
                "Limit definition inactive",
                "An inactive limit definition cannot be added to a plan."
            );
        if (_limits.Any(l => l.LimitKey == definition.LimitKey))
            throw new DuplicateResourceException(
                "PLAN_LIMIT_EXISTS",
                "Plan limit exists",
                "The plan already defines this limit."
            );
        var limit = new PlanLimit(Id, definition, limitValue, featureEnabled, mode);
        _limits.Add(limit);
        return limit;
    }

    public void RemoveLimit(string limitKey)
    {
        EnsureDraft();
        var limit =
            _limits.FirstOrDefault(l => l.LimitKey == limitKey)
            ?? throw new ResourceNotFoundException(
                "PLAN_LIMIT_NOT_FOUND",
                "Plan limit not found",
                "The plan does not define this limit."
            );
        _limits.Remove(limit);
    }

    public void Publish(DateTimeOffset now)
    {
        EnsureDraft();
        Status = PlanStatus.Published;
        PublishedAt = now;
    }

    public void Retire()
    {
        if (Status != PlanStatus.Published)
            throw new BusinessRuleException(
                "PLAN_INVALID_STATE",
                "Invalid plan state",
                "Only a published plan can be retired."
            );
        Status = PlanStatus.Retired;
    }

    private void EnsureDraft()
    {
        if (Status != PlanStatus.Draft)
            throw new BusinessRuleException(
                "PLAN_NOT_DRAFT",
                "Plan not editable",
                "Published plan versions are immutable; create a new version."
            );
    }
}

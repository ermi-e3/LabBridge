using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.SubscriptionManagement.Enums;

namespace LabBridge.Domain.Features.SubscriptionManagement.Entities;

/// <summary>
/// The organization's single commercial subscription (never deleted; Cancelled is a status). Commercial state never changes organization status or
/// configuration (BR-SUB-004). ASSUMPTION A-SUB-01: every plan/status change requires a reason (BR-SUB-009 says "where specified").
/// Aggregate root of its history.
/// </summary>
public sealed class Subscription : AggregateRoot, IOrganizationOwned
{
    private readonly List<SubscriptionHistory> _history = [];

    private Subscription() { }

    public Guid OrganizationId { get; private set; }
    public Guid PlanId { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateOnly PeriodAnchorDate { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
    public Guid? ChangedBy { get; private set; }

    public IReadOnlyCollection<SubscriptionHistory> History => _history.AsReadOnly();

    public static Subscription Assign(
        Guid organizationId,
        Plan plan,
        SubscriptionStatus status,
        DateOnly periodAnchorDate,
        string reason,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        ArgumentNullException.ThrowIfNull(plan);
        EnsureAssignable(plan);
        var s = new Subscription
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
            PlanId = plan.Id,
            Status = Guard.Defined(status, nameof(status)),
            PeriodAnchorDate = periodAnchorDate,
            ChangedAt = now,
            ChangedBy = actorId,
        };
        s._history.Add(
            new SubscriptionHistory(
                organizationId,
                s.Id,
                null,
                plan.Id,
                null,
                status,
                Guard.Required(reason, nameof(reason), 500),
                actorId,
                now
            )
        );
        return s;
    }

    public void ChangePlan(Plan plan, string reason, DateTimeOffset now, Guid? actorId)
    {
        ArgumentNullException.ThrowIfNull(plan);
        EnsureAssignable(plan);
        var validReason = Guard.Required(reason, nameof(reason), 500);   // validate before any state changes
        if (plan.Id == PlanId)
            throw new ConflictException(
                "SUBSCRIPTION_NO_CHANGE",
                "No change",
                "The subscription already uses this plan version."
            );
        var from = PlanId;
        PlanId = plan.Id;
        Record(from, Status, reason, now, actorId);
    }

    public void ChangeStatus(
        SubscriptionStatus status,
        string reason,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        Guard.Defined(status, nameof(status));
            var validReason = Guard.Required(reason, nameof(reason), 500);   // CHANGE: validate before any state changes
        if (status == Status)
            throw new ConflictException(
                "SUBSCRIPTION_NO_CHANGE",
                "No change",
                "The subscription already has this status."
            );
        var fromStatus = Status;
        Status = status;
        _history.Add(
            new SubscriptionHistory(
                OrganizationId,
                Id,
                PlanId,
                PlanId,
                fromStatus,
                status,
                Guard.Required(reason, nameof(reason), 500),
                actorId,
                now
            )
        );
        ChangedAt = now;
        ChangedBy = actorId;
    }

    private void Record(
        Guid fromPlan,
        SubscriptionStatus status,
        string validReason,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        _history.Add(new SubscriptionHistory(OrganizationId, Id, fromPlan, PlanId, status, status, validReason, actorId, now));
        ChangedAt = now;
        ChangedBy = actorId;
    }

    private static void EnsureAssignable(Plan plan)
    {
        if (plan.Status != PlanStatus.Published)
            throw new BusinessRuleException(
                "PLAN_NOT_ASSIGNABLE",
                "Plan not assignable",
                "Only a Published plan version can be assigned."
            );
    }
}

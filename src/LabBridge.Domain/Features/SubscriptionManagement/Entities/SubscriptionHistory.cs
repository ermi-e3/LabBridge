using LabBridge.Domain.Common;
using LabBridge.Domain.Features.SubscriptionManagement.Enums;

namespace LabBridge.Domain.Features.SubscriptionManagement.Entities;

/// <summary>Append-only record of subscription plan/status changes.</summary>
public sealed class SubscriptionHistory : Entity, IOrganizationOwned
{
    private SubscriptionHistory() { }

    internal SubscriptionHistory(
        Guid organizationId,
        Guid subscriptionId,
        Guid? fromPlanId,
        Guid toPlanId,
        SubscriptionStatus? fromStatus,
        SubscriptionStatus toStatus,
        string? reason,
        Guid? actorId,
        DateTimeOffset changedAt
    )
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        SubscriptionId = subscriptionId;
        FromPlanId = fromPlanId;
        ToPlanId = toPlanId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        Reason = Guard.Optional(reason, nameof(reason), 500);
        ActorId = actorId;
        ChangedAt = changedAt;
    }

    public Guid OrganizationId { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public Guid? FromPlanId { get; private set; }
    public Guid ToPlanId { get; private set; }
    public SubscriptionStatus? FromStatus { get; private set; }
    public SubscriptionStatus ToStatus { get; private set; }
    public string? Reason { get; private set; }
    public Guid? ActorId { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
}

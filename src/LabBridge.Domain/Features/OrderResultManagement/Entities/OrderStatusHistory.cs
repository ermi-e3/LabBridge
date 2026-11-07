using LabBridge.Domain.Common;
using LabBridge.Domain.Features.OrderResultManagement.Enums;

namespace LabBridge.Domain.Features.OrderResultManagement.Entities;

/// <summary>Insert-only order status history (Confidential). Never rewritten.</summary>
public sealed class OrderStatusHistory : Entity, IOrganizationOwned
{
    private OrderStatusHistory() { }

    internal OrderStatusHistory(
        Guid organizationId,
        Guid orderId,
        OrderStatus? from,
        OrderStatus to,
        string? reason,
        StatusActorType actorType,
        Guid? actorId,
        StatusSource source,
        Guid? deliveryId,
        Guid? correlationId,
        DateTimeOffset changedAt
    )
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        OrderId = orderId;
        FromStatus = from;
        ToStatus = to;
        Reason = Guard.Optional(reason, nameof(reason), 500);
        ActorType = actorType;
        ActorId = actorId;
        Source = source;
        DeliveryId = deliveryId;
        CorrelationId = correlationId;
        ChangedAt = changedAt;
    }

    public Guid OrganizationId { get; private set; }
    public Guid OrderId { get; private set; }
    public OrderStatus? FromStatus { get; private set; }
    public OrderStatus ToStatus { get; private set; }
    public string? Reason { get; private set; }
    public StatusActorType ActorType { get; private set; }
    public Guid? ActorId { get; private set; }
    public StatusSource Source { get; private set; }
    public Guid? DeliveryId { get; private set; }
    public Guid? CorrelationId { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
}

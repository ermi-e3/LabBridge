using LabBridge.Domain.Features.OrderResultManagement.Enums;

namespace LabBridge.Domain.Features.OrderResultManagement;

/// <summary>Immutable lookup of permitted status transitions.</summary>
public sealed class StatusTransitionTable<TStatus>
    where TStatus : struct, Enum
{
    private readonly Dictionary<TStatus, HashSet<TStatus>> _allowed;

    public StatusTransitionTable(params (TStatus From, TStatus[] To)[] rules) =>
        _allowed = rules.ToDictionary(r => r.From, r => r.To.ToHashSet());

    public bool IsAllowed(TStatus from, TStatus to) =>
        _allowed.TryGetValue(from, out var set) && set.Contains(to);
}

/// <summary>Transition tables of Module 7 section 4.0. Quarantined -> prior delivery state (delivery quarantine release) is handled by the aggregates.</summary>
public static class WorkflowTransitions
{
    public static readonly StatusTransitionTable<OrderStatus> Order = new(
        (OrderStatus.Received, [OrderStatus.Validated, OrderStatus.Quarantined]),
        (OrderStatus.Validated, [OrderStatus.Mapped, OrderStatus.MappingFailed]),
        (OrderStatus.MappingFailed, [OrderStatus.Mapped]),
        (OrderStatus.Mapped, [OrderStatus.ReadyForDelivery]),
        (OrderStatus.ReadyForDelivery, [OrderStatus.Delivering, OrderStatus.Quarantined]),
        (
            OrderStatus.Delivering,
            [
                OrderStatus.Sent,
                OrderStatus.DeliveryFailed,
                OrderStatus.DeliveryUncertain,
                OrderStatus.ReadyForDelivery,
                OrderStatus.Quarantined,
            ]
        ),
        (
            OrderStatus.Sent,
            [OrderStatus.Acknowledged, OrderStatus.DeliveryUncertain, OrderStatus.DeliveryFailed]
        ),
        (OrderStatus.Acknowledged, [OrderStatus.Accepted, OrderStatus.DeliveryFailed]),
        (
            OrderStatus.DeliveryUncertain,
            [
                OrderStatus.Sent,
                OrderStatus.Acknowledged,
                OrderStatus.Accepted,
                OrderStatus.ReadyForDelivery,
                OrderStatus.DeliveryFailed,
            ]
        ),
        (OrderStatus.DeliveryFailed, [OrderStatus.ReadyForDelivery]),
        (OrderStatus.Quarantined, [OrderStatus.Received, OrderStatus.Rejected])
    );

    /// <summary>ASSUMPTION A-WFL-02: DeliveryUncertain -> DestinationRejected and DeliveryFailed from Acknowledged follow the order table by analogy; the diagram does not draw them.</summary>
    public static readonly StatusTransitionTable<ResultStatus> Result = new(
        (
            ResultStatus.Received,
            [ResultStatus.Unmatched, ResultStatus.Quarantined, ResultStatus.Validated]
        ),
        (ResultStatus.Unmatched, [ResultStatus.Validated, ResultStatus.Rejected]),
        (ResultStatus.Validated, [ResultStatus.Normalized]),
        (ResultStatus.Normalized, [ResultStatus.Mapped, ResultStatus.MappingFailed]),
        (ResultStatus.MappingFailed, [ResultStatus.Mapped]),
        (ResultStatus.Mapped, [ResultStatus.ReadyForDelivery]),
        (ResultStatus.ReadyForDelivery, [ResultStatus.Delivering, ResultStatus.Quarantined]),
        (
            ResultStatus.Delivering,
            [
                ResultStatus.Sent,
                ResultStatus.DeliveryFailed,
                ResultStatus.DeliveryUncertain,
                ResultStatus.ReadyForDelivery,
                ResultStatus.Quarantined,
            ]
        ),
        (
            ResultStatus.Sent,
            [
                ResultStatus.Acknowledged,
                ResultStatus.DestinationRejected,
                ResultStatus.DeliveryUncertain,
                ResultStatus.DeliveryFailed,
            ]
        ),
        (
            ResultStatus.Acknowledged,
            [ResultStatus.Accepted, ResultStatus.DestinationRejected, ResultStatus.DeliveryFailed]
        ),
        (
            ResultStatus.DeliveryUncertain,
            [
                ResultStatus.Sent,
                ResultStatus.Acknowledged,
                ResultStatus.Accepted,
                ResultStatus.ReadyForDelivery,
                ResultStatus.DeliveryFailed,
                ResultStatus.DestinationRejected,
            ]
        ),
        (ResultStatus.DeliveryFailed, [ResultStatus.ReadyForDelivery]),
        (ResultStatus.Quarantined, [ResultStatus.Received, ResultStatus.Rejected])
    );
}

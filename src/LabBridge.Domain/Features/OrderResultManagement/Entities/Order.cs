using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.OrderResultManagement.Enums;
using LabBridge.Domain.Common.ValueObjects;

namespace LabBridge.Domain.Features.OrderResultManagement.Entities;

/// <summary>
/// A laboratory order received from an external system, tracked until delivered to the instrument. Module 7 is the single writer of its status;
/// every transition is compare-and-set on the expected status and appended to the history (BR-WFL-009). Aggregate root of lines and history.
/// PlacerOrderId is unique per (organization, source system) while the order is not Quarantined/Rejected. Patient data is protected (Confidential).
/// </summary>
public sealed class Order : AuditedAggregateRoot, IOrganizationOwned
{
    private readonly List<OrderLine> _lines = [];
    private readonly List<OrderStatusHistory> _statusHistory = [];

    private Order() { }

    public Guid OrganizationId { get; private set; }
    public Guid CorrelationId { get; private set; }
    public Guid SourceConnectorId { get; private set; }
    public Guid SourceExternalSystemId { get; private set; }
    public Guid? RouteId { get; private set; }
    public Guid? TargetInstrumentId { get; private set; }
    public string? PlacerOrderId { get; private set; }
    public string? FillerOrderId { get; private set; }
    public string? AccessionNumber { get; private set; }
    public string? SpecimenId { get; private set; }
    public string? MessageControlId { get; private set; }
    public string? PatientExternalId { get; private set; }
    public string? PatientName { get; private set; }
    public DateOnly? PatientDateOfBirth { get; private set; }
    public string? PatientSex { get; private set; }
    public string? SpecimenType { get; private set; }
    public string? SpecimenTypeCanonical { get; private set; }
    public string? SpecimenTypeTarget { get; private set; }
    public DateTimeOffset? SpecimenCollectedAt { get; private set; }
    public string? SpecimenCollectedAtRaw { get; private set; }
    public string? Priority { get; private set; }
    public string? PriorityCanonical { get; private set; }
    public string? PriorityTarget { get; private set; }
    public DateTimeOffset? OrderedAt { get; private set; }
    public string? OrderedAtRaw { get; private set; }
    public OrderStatus Status { get; private set; }
    public string? StatusReason { get; private set; }
    public QuarantineReason? QuarantineReason { get; private set; }
    public OrderStatus? QuarantinedFromStatus { get; private set; }
    public Guid? ConflictsWithOrderId { get; private set; }
    public Guid SourceMessageId { get; private set; }
    public Guid NormalizedMessageId { get; private set; }
    public string ContentHash { get; private set; } = null!;
    public Guid? MappingRunId { get; private set; }
    public int PayloadVersion { get; private set; }
    public bool IsSimulated { get; private set; }

    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();
    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    /// <summary>Creates an order in status Received (FR-WFL create). At least one line is required.</summary>
    public static Order Create(
        Guid organizationId,
        Guid correlationId,
        Guid sourceConnectorId,
        Guid sourceExternalSystemId,
        Guid sourceMessageId,
        Guid normalizedMessageId,
        string contentHash,
        bool isSimulated,
        OrderData data,
        IEnumerable<(
            int LineNumber,
            string TestCode,
            string? CodeSystem,
            string? Description
        )> lines,
        DateTimeOffset now
    )
    {
        ArgumentNullException.ThrowIfNull(data);
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
            CorrelationId = Guard.NotEmpty(correlationId, nameof(correlationId)),
            SourceConnectorId = Guard.NotEmpty(sourceConnectorId, nameof(sourceConnectorId)),
            SourceExternalSystemId = Guard.NotEmpty(
                sourceExternalSystemId,
                nameof(sourceExternalSystemId)
            ),
            SourceMessageId = Guard.NotEmpty(sourceMessageId, nameof(sourceMessageId)),
            NormalizedMessageId = Guard.NotEmpty(normalizedMessageId, nameof(normalizedMessageId)),
            //ContentHash = Guard.Required(contentHash, nameof(contentHash), 64),
            ContentHash = Sha256Hash.Create(contentHash).Value,
            IsSimulated = isSimulated,
            PayloadVersion = 1,
            Status = OrderStatus.Received,
            PlacerOrderId = Guard.Optional(data.PlacerOrderId, nameof(data.PlacerOrderId), 100),
            FillerOrderId = Guard.Optional(data.FillerOrderId, nameof(data.FillerOrderId), 100),
            AccessionNumber = Guard.Optional(
                data.AccessionNumber,
                nameof(data.AccessionNumber),
                100
            ),
            SpecimenId = Guard.Optional(data.SpecimenId, nameof(data.SpecimenId), 100),
            MessageControlId = Guard.Optional(
                data.MessageControlId,
                nameof(data.MessageControlId),
                100
            ),
            PatientExternalId = Guard.Optional(
                data.PatientExternalId,
                nameof(data.PatientExternalId),
                100
            ),
            PatientName = Guard.Optional(data.PatientName, nameof(data.PatientName), 200),
            PatientDateOfBirth = data.PatientDateOfBirth,
            PatientSex = Guard.Optional(data.PatientSex, nameof(data.PatientSex), 20),
            SpecimenType = Guard.Optional(data.SpecimenType, nameof(data.SpecimenType), 100),
            SpecimenCollectedAt = data.SpecimenCollectedAt,
            SpecimenCollectedAtRaw = Guard.Optional(
                data.SpecimenCollectedAtRaw,
                nameof(data.SpecimenCollectedAtRaw),
                100
            ),
            Priority = Guard.Optional(data.Priority, nameof(data.Priority), 50),
            OrderedAt = data.OrderedAt,
            OrderedAtRaw = Guard.Optional(data.OrderedAtRaw, nameof(data.OrderedAtRaw), 100),
        };
        foreach (var l in lines)
            order._lines.Add(
                new OrderLine(
                    organizationId,
                    order.Id,
                    l.LineNumber,
                    l.TestCode,
                    l.CodeSystem,
                    l.Description
                )
            );
        if (order._lines.Count == 0)
            throw new ValidationException(
                "ORDER_LINES_REQUIRED",
                "Order lines required",
                "An order must contain at least one line."
            );
        if (order._lines.Select(l => l.LineNumber).Distinct().Count() != order._lines.Count)
            throw new ValidationException(
                "ORDER_LINE_NUMBERS_DUPLICATE",
                "Duplicate line numbers",
                "Order line numbers must be unique."
            );
        order.StampCreated(now, null);
        order._statusHistory.Add(
            new OrderStatusHistory(
                organizationId,
                order.Id,
                null,
                OrderStatus.Received,
                null,
                StatusActorType.System,
                null,
                StatusSource.WFL,
                null,
                correlationId,
                now
            )
        );
        return order;
    }

    /// <summary>Records the resolved route and instrument.</summary>
    //public void ResolveRoute(Guid routeId, Guid targetInstrumentId)
    //{
    //    RouteId = Guard.NotEmpty(routeId, nameof(routeId));
    //    TargetInstrumentId = Guard.NotEmpty(targetInstrumentId, nameof(targetInstrumentId));
    //}

    public void ResolveRoute(Guid routeId, Guid targetInstrumentId, DateTimeOffset now, Guid? actorId)
    {
        EnsureBeforeDelivery("Resolving the route");
        RouteId = Guard.NotEmpty(routeId, nameof(routeId));
        TargetInstrumentId = Guard.NotEmpty(targetInstrumentId, nameof(targetInstrumentId));
        StampUpdated(now, actorId);
    }

    /// <summary>Stores mapping output: canonical/target specimen type and priority and the per-line instrument codes.</summary>
    public void ApplyMapping(
        Guid mappingRunId,
        string? specimenTypeCanonical,
        string? specimenTypeTarget,
        string? priorityCanonical,
        string? priorityTarget,
        IReadOnlyDictionary<int, (string? Canonical, string? Instrument)> lineCodes, DateTimeOffset now, Guid actorId
    )
    {
        //MappingRunId = Guard.NotEmpty(mappingRunId, nameof(mappingRunId));
        EnsureBeforeDelivery("Applying the mapping");
        MappingRunId = Guard.NotEmpty(mappingRunId, nameof(mappingRunId));
        SpecimenTypeCanonical = Guard.Optional(
            specimenTypeCanonical,
            nameof(specimenTypeCanonical),
            100
        );
        SpecimenTypeTarget = Guard.Optional(specimenTypeTarget, nameof(specimenTypeTarget), 100);
        PriorityCanonical = Guard.Optional(priorityCanonical, nameof(priorityCanonical), 50);
        PriorityTarget = Guard.Optional(priorityTarget, nameof(priorityTarget), 50);
        foreach (var line in _lines)
            if (lineCodes.TryGetValue(line.LineNumber, out var codes))
                line.ApplyMapping(codes.Canonical, codes.Instrument);

        StampUpdated(now, actorId);
    }

    /// <summary>
    /// Compare-and-set status change (BR-WFL-009). <paramref name="expectedFrom"/> must equal the current status, otherwise the caller lost a race
    /// (ConflictException). Quarantine uses <see cref="Quarantine"/>.
    /// </summary>
    public void TransitionTo(
        OrderStatus expectedFrom,
        OrderStatus to,
        string? reason,
        StatusActorType actorType,
        Guid? actorId,
        StatusSource source,
        Guid? deliveryId,
        Guid? correlationId,
        DateTimeOffset now
    )
    {
        if (to == OrderStatus.Quarantined)
            throw new BusinessRuleException(
                "ORDER_USE_QUARANTINE",
                "Use quarantine operation",
                "Quarantine requires a reason code; use Quarantine()."
            );
        Apply(
            expectedFrom,
            to,
            reason,
            actorType,
            actorId,
            source,
            deliveryId,
            correlationId,
            now,
            allowReturnFromQuarantine: false
        );
    }

    public void Quarantine(
        OrderStatus expectedFrom,
        QuarantineReason reason,
        Guid? conflictsWithOrderId,
        string? detail,
        StatusActorType actorType,
        Guid? actorId,
        StatusSource source,
        Guid? deliveryId,
        Guid? correlationId,
        DateTimeOffset now
    )
    {
        Apply(
            expectedFrom,
            OrderStatus.Quarantined,
            detail,
            actorType,
            actorId,
            source,
            deliveryId,
            correlationId,
            now,
            false
        );
        QuarantineReason = Guard.Defined(reason, nameof(reason));
        QuarantinedFromStatus = expectedFrom;
        ConflictsWithOrderId = conflictsWithOrderId;
    }

    /// <summary>Releases a quarantined order: delivery quarantine returns to the prior delivery state; other reasons return to Received (BR-WFL-016).</summary>
    public void ReleaseFromQuarantine(
        string reason,
        Guid actorId,
        StatusSource source,
        Guid? deliveryId,
        Guid? correlationId,
        DateTimeOffset now
    )
    {
        if (Status != OrderStatus.Quarantined)
            throw new BusinessRuleException(
                "ORDER_INVALID_STATE",
                "Invalid order state",
                "Only a quarantined order can be released."
            );
        var target =
            QuarantineReason == Enums.QuarantineReason.DeliveryQuarantine
            && QuarantinedFromStatus is { } prior
                ? prior
                : OrderStatus.Received;
        Apply(
            OrderStatus.Quarantined,
            target,
            Guard.Required(reason, nameof(reason), 500),
            StatusActorType.User,
            actorId,
            source,
            deliveryId,
            correlationId,
            now,
            true
        );
        QuarantineReason = null;
        QuarantinedFromStatus = null;
        ConflictsWithOrderId = null;
    }

    /// <summary>Route and mapping must not change once the order is in the delivery phase, quarantined or rejected.</summary>
    private void EnsureBeforeDelivery(string action)
    {
        if (Status is not (OrderStatus.Received or OrderStatus.Validated or OrderStatus.MappingFailed or OrderStatus.Mapped))
            throw new BusinessRuleException(
                "ORDER_NOT_MODIFIABLE",
                "Order not modifiable",
                $"{action} is not allowed once the order has entered the delivery phase, been quarantined or rejected."
            );
    }

    private void Apply(
        OrderStatus expectedFrom,
        OrderStatus to,
        string? reason,
        StatusActorType actorType,
        Guid? actorId,
        StatusSource source,
        Guid? deliveryId,
        Guid? correlationId,
        DateTimeOffset now,
        bool allowReturnFromQuarantine
    )
    {
        if (Status != expectedFrom)
            throw new ConflictException(
                "ORDER_STATUS_CHANGED",
                "Order status changed",
                "The order status changed since it was read."
            );
        var permitted =
            WorkflowTransitions.Order.IsAllowed(Status, to)
            || allowReturnFromQuarantine
                && Status == OrderStatus.Quarantined
                && to is OrderStatus.ReadyForDelivery or OrderStatus.Delivering;
        if (!permitted)
            throw new BusinessRuleException(
                "ORDER_INVALID_TRANSITION",
                "Invalid order transition",
                $"An order in status {Status} cannot move to {to}."
            );
        var from = Status;
        Status = to;
        StatusReason = Guard.Optional(reason, nameof(reason), 500);
        StampUpdated(now, actorType == StatusActorType.User ? actorId : null);
        _statusHistory.Add(
            new OrderStatusHistory(
                OrganizationId,
                Id,
                from,
                to,
                StatusReason,
                actorType,
                actorId,
                source,
                deliveryId,
                correlationId,
                now
            )
        );
    }
}

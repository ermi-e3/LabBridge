using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Common.ValueObjects;
using LabBridge.Domain.Features.OrderResultManagement.Enums;

namespace LabBridge.Domain.Features.OrderResultManagement.Entities;

/// <summary>
/// A laboratory result from an instrument, correlated to an order when possible. Module 7 is the single status writer (compare-and-set).
/// Unmatched/ambiguous results are held, never silently discarded. A possible correction is quarantined for review (never auto-applied).
/// Aggregate root of observations and status history.
/// </summary>
public sealed class Result : AuditedAggregateRoot, IOrganizationOwned
{
    private readonly List<Observation> _observations = [];
    private readonly List<ResultStatusHistory> _statusHistory = [];
    private readonly List<string> _enrichedFields = [];

    private Result() { }

    public Guid OrganizationId { get; private set; }
    public Guid CorrelationId { get; private set; }
    public Guid SourceConnectorId { get; private set; }
    public Guid SourceInstrumentId { get; private set; }
    public Guid? RouteId { get; private set; }
    public Guid? DestinationExternalSystemId { get; private set; }
    public Guid? OrderId { get; private set; }
    public CorrelationStatus CorrelationStatus { get; private set; }
    public CorrelationMethod? CorrelationMethod { get; private set; }
    public string? CorrelationBasis { get; private set; }
    public DateTimeOffset? CorrelatedAt { get; private set; }
    public Guid? CorrelatedBy { get; private set; }
    public string? PlacerOrderId { get; private set; }
    public string? FillerOrderId { get; private set; }
    public string? AccessionNumber { get; private set; }
    public string? SpecimenId { get; private set; }
    public string? MessageControlId { get; private set; }
    public IReadOnlyCollection<string> EnrichedFields => _enrichedFields.AsReadOnly();
    public ResultStatus Status { get; private set; }
    public string? StatusReason { get; private set; }
    public QuarantineReason? QuarantineReason { get; private set; }
    public ResultStatus? QuarantinedFromStatus { get; private set; }
    public bool PossibleCorrection { get; private set; }
    public Guid SourceMessageId { get; private set; }
    public Guid NormalizedMessageId { get; private set; }
    public string ContentHash { get; private set; } = null!;
    public Guid? MappingRunId { get; private set; }
    public int PayloadVersion { get; private set; }
    public bool IsSimulated { get; private set; }

    public IReadOnlyCollection<Observation> Observations => _observations.AsReadOnly();
    public IReadOnlyCollection<ResultStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    /// <summary>Creates a result in status Received with the correlation outcome evaluated by the caller (default Unmatched).</summary>
    public static Result Create(
        Guid organizationId,
        Guid correlationId,
        Guid sourceConnectorId,
        Guid sourceInstrumentId,
        Guid sourceMessageId,
        Guid normalizedMessageId,
        string contentHash,
        bool isSimulated,
        ResultData ids,
        IEnumerable<ObservationData> observations,
        DateTimeOffset now
    )
    {
        ArgumentNullException.ThrowIfNull(ids);
        var result = new Result
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
            CorrelationId = Guard.NotEmpty(correlationId, nameof(correlationId)),
            SourceConnectorId = Guard.NotEmpty(sourceConnectorId, nameof(sourceConnectorId)),
            SourceInstrumentId = Guard.NotEmpty(sourceInstrumentId, nameof(sourceInstrumentId)),
            SourceMessageId = Guard.NotEmpty(sourceMessageId, nameof(sourceMessageId)),
            NormalizedMessageId = Guard.NotEmpty(normalizedMessageId, nameof(normalizedMessageId)),
            //ContentHash = Guard.Required(contentHash, nameof(contentHash), 64),
            ContentHash = Sha256Hash.Create(contentHash).Value,
            IsSimulated = isSimulated,
            PayloadVersion = 1,
            Status = ResultStatus.Received,
            CorrelationStatus = CorrelationStatus.Unmatched,
            PlacerOrderId = Guard.Optional(ids.PlacerOrderId, nameof(ids.PlacerOrderId), 100),
            FillerOrderId = Guard.Optional(ids.FillerOrderId, nameof(ids.FillerOrderId), 100),
            AccessionNumber = Guard.Optional(ids.AccessionNumber, nameof(ids.AccessionNumber), 100),
            SpecimenId = Guard.Optional(ids.SpecimenId, nameof(ids.SpecimenId), 100),
            MessageControlId = Guard.Optional(
                ids.MessageControlId,
                nameof(ids.MessageControlId),
                100
            ),
        };
        foreach (var o in observations)
            result._observations.Add(new Observation(organizationId, result.Id, o));
        if (result._observations.Count == 0)
            throw new ValidationException(
                "RESULT_OBSERVATIONS_REQUIRED",
                "Observations required",
                "A result must contain at least one observation."
            );
        if (
            result._observations.Select(o => o.LineNumber).Distinct().Count()
            != result._observations.Count
        )
            throw new ValidationException(
                "RESULT_LINE_NUMBERS_DUPLICATE",
                "Duplicate line numbers",
                "Observation line numbers must be unique."
            );
        result.StampCreated(now, null);
        result._statusHistory.Add(
            new ResultStatusHistory(
                organizationId,
                result.Id,
                null,
                ResultStatus.Received,
                null,
                StatusActorType.System,
                null,
                StatusSource.WFL,
                null,
                correlationId,
                now
            )
        );
        return result;
    }

    /// <summary>Links the result to an order (automatic or manual). Observations are linked to lines through the supplied map (observation line number -> order line id).</summary>
    public void Correlate(
        Order order,
        CorrelationMethod method,
        string? basis,
        Guid? correlatedBy,
        IReadOnlyDictionary<int, Guid> observationToOrderLine,
        DateTimeOffset now
    )
    {
        //ArgumentNullException.ThrowIfNull(order);
        //if (order.OrganizationId != OrganizationId)
        //    throw new ForbiddenException(
        //        "TENANT_ACCESS_DENIED",
        //        "Tenant access denied",
        //        "The order does not belong to the organization."
        //    );
        //if (method == Enums.CorrelationMethod.Manual && correlatedBy is null)
        //    throw new ValidationException(
        //        "RESULT_CORRELATOR_REQUIRED",
        //        "Correlating user required",
        //        "Manual correlation must record the user."
        //    );

        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(observationToOrderLine);
        EnsureBeforeDelivery("Changing the correlation");
        if (order.OrganizationId != OrganizationId)
            throw new ForbiddenException("TENANT_ACCESS_DENIED", "Tenant access denied", "The order does not belong to the organization.");
        if (method == Enums.CorrelationMethod.Manual && correlatedBy is null)
            throw new ValidationException("RESULT_CORRELATOR_REQUIRED", "Correlating user required", "Manual correlation must record the user.");


        //OrderId = order.Id;
        //CorrelationStatus = CorrelationStatus.Matched;
        //CorrelationMethod = Guard.Defined(method, nameof(method));
        //CorrelationBasis = Guard.Optional(basis, nameof(basis), 100);
        //CorrelatedAt = now;
        //CorrelatedBy = correlatedBy;
        //foreach (var o in _observations)
        //    o.LinkOrderLine(
        //        observationToOrderLine.TryGetValue(o.LineNumber, out var lineId) ? lineId : null
        //    );

        // CHANGE: the database cannot tie an observation's line to this result's order, so the domain checks it (order.Lines must be loaded).
        var orderLineIds = order.Lines.Select(l => l.Id).ToHashSet();
        if (observationToOrderLine.Values.Any(lineId => !orderLineIds.Contains(lineId)))
            throw new ValidationException("RESULT_LINE_NOT_IN_ORDER", "Line not in order", "An observation is linked to a line that does not belong to the order.");

        OrderId = order.Id;
        CorrelationStatus = CorrelationStatus.Matched;
        CorrelationMethod = Guard.Defined(method, nameof(method));
        CorrelationBasis = Guard.Optional(basis, nameof(basis), 100);
        CorrelatedAt = now;
        CorrelatedBy = correlatedBy;
        foreach (var o in _observations)
            o.LinkOrderLine(observationToOrderLine.TryGetValue(o.LineNumber, out var lineId) ? lineId : null);
        StampUpdated(now, correlatedBy);
    }

    //public void MarkUnmatched(bool ambiguous)
    //{
    //    OrderId = null;
    //    CorrelationStatus = ambiguous ? CorrelationStatus.Ambiguous : CorrelationStatus.Unmatched;
    //}

    public void MarkUnmatched(bool ambiguous, DateTimeOffset now, Guid? actorId)
    {
        EnsureBeforeDelivery("Changing the correlation");
        OrderId = null;
        CorrelationStatus = ambiguous ? CorrelationStatus.Ambiguous : CorrelationStatus.Unmatched;
        // CHANGE: a result that is not matched must not keep evidence of an earlier match.
        CorrelationMethod = null;
        CorrelationBasis = null;
        CorrelatedAt = null;
        CorrelatedBy = null;
        foreach (var o in _observations)
            o.LinkOrderLine(null);
        StampUpdated(now, null);
    }

    public void ResolveRoute(Guid routeId, Guid destinationExternalSystemId, DateTimeOffset now, Guid? actorId)
    {
        RouteId = Guard.NotEmpty(routeId, nameof(routeId));
        DestinationExternalSystemId = Guard.NotEmpty(
            destinationExternalSystemId,
            nameof(destinationExternalSystemId)
        );
        StampUpdated(now, actorId);

    }

    /// <summary>Records identifiers copied from the correlated order to fill gaps in the result message (VR enrichment).</summary>
    public void Enrich(string? placerOrderId, string? accessionNumber, string? specimenId, DateTimeOffset now, Guid? actorId)
    {
        void Fill(string field, string? current, string? value, Action<string> set)
        {
            if (string.IsNullOrWhiteSpace(current) && !string.IsNullOrWhiteSpace(value))
            {
                set(value.Trim());
                _enrichedFields.Add(field);
            }
        }
        Fill(nameof(PlacerOrderId), PlacerOrderId, placerOrderId, v => PlacerOrderId = v);
        Fill(nameof(AccessionNumber), AccessionNumber, accessionNumber, v => AccessionNumber = v);
        Fill(nameof(SpecimenId), SpecimenId, specimenId, v => SpecimenId = v);

        StampUpdated(now, actorId);

    }

    public void ApplyMapping(
        Guid mappingRunId,
        IReadOnlyDictionary<int, ObservationMapping> byLineNumber, DateTimeOffset now, Guid? actorId
    )
    {
        MappingRunId = Guard.NotEmpty(mappingRunId, nameof(mappingRunId));
        foreach (var o in _observations)
            if (byLineNumber.TryGetValue(o.LineNumber, out var m))
                o.ApplyMapping(
                    m.CanonicalTestCode,
                    m.DestinationTestCode,
                    m.CanonicalUnit,
                    m.DestinationUnit,
                    m.DestinationValueText,
                    m.DestinationValueNumeric,
                    m.ConversionRuleId
                );
                StampUpdated(now, actorId);

    }

    /// <summary>Flags a result that may correct an earlier one; it is quarantined, never auto-applied.</summary>
    public void FlagPossibleCorrection(DateTimeOffset now, Guid? actorId){ 
        
       PossibleCorrection = true;
       StampUpdated(now, actorId);
    }

    public void TransitionTo(
        ResultStatus expectedFrom,
        ResultStatus to,
        string? reason,
        StatusActorType actorType,
        Guid? actorId,
        StatusSource source,
        Guid? deliveryId,
        Guid? correlationId,
        DateTimeOffset now
    )
    {
        if (to == ResultStatus.Quarantined)
            throw new BusinessRuleException(
                "RESULT_USE_QUARANTINE",
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
            false
        );
    }

    public void Quarantine(
        ResultStatus expectedFrom,
        QuarantineReason reason,
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
            ResultStatus.Quarantined,
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
    }

    public void ReleaseFromQuarantine(
        string reason,
        Guid actorId,
        StatusSource source,
        Guid? deliveryId,
        Guid? correlationId,
        DateTimeOffset now
    )
    {
        if (Status != ResultStatus.Quarantined)
            throw new BusinessRuleException(
                "RESULT_INVALID_STATE",
                "Invalid result state",
                "Only a quarantined result can be released."
            );
        var target =
            QuarantineReason == Enums.QuarantineReason.DeliveryQuarantine
            && QuarantinedFromStatus is { } prior
                ? prior
                : ResultStatus.Received;
        Apply(
            ResultStatus.Quarantined,
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
    }

    private void EnsureBeforeDelivery(string action)
    {
        if (Status is not (ResultStatus.Received or ResultStatus.Unmatched or ResultStatus.Validated
            or ResultStatus.Normalized or ResultStatus.MappingFailed or ResultStatus.Mapped))
            throw new BusinessRuleException(
                "RESULT_NOT_MODIFIABLE",
                "Result not modifiable",
                $"{action} is not allowed once the result has entered the delivery phase, been quarantined or rejected."
            );
    }

    private void Apply(
        ResultStatus expectedFrom,
        ResultStatus to,
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
                "RESULT_STATUS_CHANGED",
                "Result status changed",
                "The result status changed since it was read."
            );
        var permitted =
            WorkflowTransitions.Result.IsAllowed(Status, to)
            || allowReturnFromQuarantine
                && Status == ResultStatus.Quarantined
                && to is ResultStatus.ReadyForDelivery or ResultStatus.Delivering;
        if (!permitted)
            throw new BusinessRuleException(
                "RESULT_INVALID_TRANSITION",
                "Invalid result transition",
                $"A result in status {Status} cannot move to {to}."
            );
        var from = Status;
        Status = to;
        StatusReason = Guard.Optional(reason, nameof(reason), 500);
        StampUpdated(now, actorType == StatusActorType.User ? actorId : null);
        _statusHistory.Add(
            new ResultStatusHistory(
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

/// <summary>Mapping outcome for one observation.</summary>
public sealed record ObservationMapping(
    string? CanonicalTestCode,
    string? DestinationTestCode,
    string? CanonicalUnit,
    string? DestinationUnit,
    string? DestinationValueText,
    decimal? DestinationValueNumeric,
    Guid? ConversionRuleId
);

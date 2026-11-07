using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.DeliveryManagement.Enums;
using LabBridge.Domain.Features.OrderResultManagement.Entities;
using LabBridge.Domain.Features.OrderResultManagement.Enums;

namespace LabBridge.Domain.Features.DeliveryManagement.Entities;

/// <summary>
/// The delivery work item for one DeliveryRequest (one-to-one). Owns the attempts and the state machine of section 4.0:
/// Pending -> InProgress -> Succeeded | RetryScheduled | FailedPermanent | Uncertain; Quarantined is reachable from Pending/RetryScheduled.
/// Subject (order/result) status is requested from Module 7 by the application on every state change (Module 8 never writes it directly).
/// IdempotencyKey is unique per organization. Aggregate root of attempts.
/// </summary>
public sealed class DeliveryItem : AuditedAggregateRoot, IOrganizationOwned
{
    private readonly List<DeliveryAttempt> _attempts = [];

    private DeliveryItem() { }

    public Guid OrganizationId { get; private set; }
    public Guid DeliveryRequestId { get; private set; }
    public DeliverySubjectType SubjectType { get; private set; }
    public Guid SubjectId { get; private set; }
    public int PayloadVersion { get; private set; }
    public Guid RouteId { get; private set; }
    public Guid ConnectorId { get; private set; }
    public string IdempotencyKey { get; private set; } = null!;
    public DeliveryState State { get; private set; }
    public DeliveryHoldReason? HoldReason { get; private set; }
    public DeliveryState? QuarantinedFromState { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTimeOffset? NextAttemptAt { get; private set; }
    public long ReceiptSequence { get; private set; }
    public string? ClaimedBy { get; private set; }
    public DateTimeOffset? LeaseExpiresAt { get; private set; }
    public bool IsSimulated { get; private set; }

    public IReadOnlyCollection<DeliveryAttempt> Attempts => _attempts.AsReadOnly();

    public static DeliveryItem Create(
        DeliveryRequest request,
        Guid connectorId,
        string idempotencyKey,
        bool isSimulated,
        DateTimeOffset now
    )
    {
        ArgumentNullException.ThrowIfNull(request);
        var item = new DeliveryItem
        {
            Id = Guid.NewGuid(),
            OrganizationId = request.OrganizationId,
            DeliveryRequestId = request.Id,
            SubjectType = request.SubjectType,
            SubjectId = request.SubjectId,
            PayloadVersion = request.PayloadVersion,
            RouteId = request.RouteId,
            ConnectorId = Guard.NotEmpty(connectorId, nameof(connectorId)),
            IdempotencyKey = Guard.Required(idempotencyKey, nameof(idempotencyKey), 200),
            State = DeliveryState.Pending,
            ReceiptSequence = request.ReceiptSequence,
            IsSimulated = isSimulated,
        };
        item.StampCreated(now, null);
        return item;
    }

    /// <summary>Sets the reason an item is not dispatched (Pending/RetryScheduled only, VR-DLV-003); null clears it.</summary>
    public void SetHold(DeliveryHoldReason? reason, DateTimeOffset now)
    {
        if (
            reason is not null
            && State is not (DeliveryState.Pending or DeliveryState.RetryScheduled)
        )
            throw Invalid("A hold reason is only valid for Pending or RetryScheduled items.");
        HoldReason = reason;
        StampUpdated(now, null);
    }

    /// <summary>Pending (or due RetryScheduled) -> InProgress with a new attempt carrying a fresh control identifier.</summary>
    public DeliveryAttempt StartAttempt(
        string controlId,
        int connectorConfigVersion,
        string workerId,
        DateTimeOffset leaseExpiresAt,
        DateTimeOffset now
    )
    {
        if (State is not (DeliveryState.Pending or DeliveryState.RetryScheduled))
            throw Invalid("Only a Pending or RetryScheduled item can start an attempt.");
        if (HoldReason is not null)
            throw Invalid("A held item cannot be dispatched.");
        if (State == DeliveryState.RetryScheduled && NextAttemptAt > now)
            throw Invalid("The retry is not due yet.");
        if (leaseExpiresAt <= now)
            throw new ValidationException(
                "DELIVERY_LEASE_INVALID",
                "Invalid lease",
                "The lease must expire in the future."
            );
        AttemptCount++;
        var attempt = new DeliveryAttempt(
            OrganizationId,
            Id,
            AttemptCount,
            controlId,
            connectorConfigVersion,
            now
        );
        _attempts.Add(attempt);
        ClaimedBy = Guard.Required(workerId, nameof(workerId), 100);
        LeaseExpiresAt = leaseExpiresAt;
        NextAttemptAt = null;
        State = DeliveryState.InProgress;
        StampUpdated(now, null);
        return attempt;
    }

    /// <summary>
    /// Completes the in-progress attempt and moves the item: Succeeded, RetryScheduled (requires nextAttemptAt, within maxAttempts), FailedPermanent
    /// (permanent failure or retries exhausted) or Uncertain (timeout/crash).
    /// </summary>
    public void CompleteAttempt(
        Guid attemptId,
        TransportOutcome transport,
        AckOutcome ack,
        AcceptanceOutcome acceptance,
        AttemptResult result,
        string? errorCode,
        string? errorCategory,
        int? maxAttempts,
        DateTimeOffset? nextAttemptAt,
        DateTimeOffset now
    )
    {
        if (State != DeliveryState.InProgress)
            throw Invalid("Only an in-progress item can complete an attempt.");
        var attempt =
            _attempts.FirstOrDefault(a => a.Id == attemptId)
            ?? throw new ResourceNotFoundException(
                "DELIVERY_ATTEMPT_NOT_FOUND",
                "Attempt not found",
                "The attempt does not belong to this delivery."
            );
        attempt.Complete(transport, ack, acceptance, result, errorCode, errorCategory, now);

        switch (result)
        {
            case AttemptResult.Succeeded:
                State = DeliveryState.Succeeded;
                break;
            case AttemptResult.FailedRetryable
                when maxAttempts is { } max && AttemptCount < max && nextAttemptAt is { } due:
                State = DeliveryState.RetryScheduled;
                NextAttemptAt = due;
                break;
            case AttemptResult.FailedRetryable: // exhausted, or no policy => no automatic retry (BR-DLV-008)
            case AttemptResult.FailedPermanent:
                State = DeliveryState.FailedPermanent;
                break;
            case AttemptResult.Uncertain:
                State = DeliveryState.Uncertain;
                break;
        }
        ClaimedBy = null;
        LeaseExpiresAt = null;
        StampUpdated(now, null);
    }

    /// <summary>Late acknowledgment received for an Uncertain attempt resolves the item (Uncertain -> Succeeded).</summary>
    //public void ApplyLateAcknowledgment(
    //    Guid attemptId,
    //    AckOutcome ack,
    //    AcceptanceOutcome acceptance,
    //    DateTimeOffset now
    //)
    //{
    //    if (State != DeliveryState.Uncertain)
    //        throw Invalid("Only an uncertain item can be resolved by a late acknowledgment.");
    //    var attempt =
    //        _attempts.FirstOrDefault(a => a.Id == attemptId)
    //        ?? throw new ResourceNotFoundException(
    //            "DELIVERY_ATTEMPT_NOT_FOUND",
    //            "Attempt not found",
    //            "The attempt does not belong to this delivery."
    //        );
    //    var positive = ack == AckOutcome.Positive;
    //    attempt.ApplyEvidence(
    //        ack,
    //        acceptance,
    //        positive ? AttemptResult.Succeeded : AttemptResult.FailedPermanent
    //    );
    //    State = positive ? DeliveryState.Succeeded : DeliveryState.FailedPermanent;
    //    StampUpdated(now, null);
    //}

    public void ApplyLateAcknowledgment(Guid attemptId, AckOutcome ack, AcceptanceOutcome acceptance, DateTimeOffset now)
    {
        if (State != DeliveryState.Uncertain)
            throw Invalid("Only an uncertain item can be resolved by a late acknowledgment.");
        if (ack is not (AckOutcome.Positive or AckOutcome.Negative))
            throw new ValidationException("DELIVERY_ACK_NOT_FINAL", "Acknowledgment not final",
                "A late acknowledgment must be positive or negative.");
        var attempt = FindAttempt(attemptId);
        var positive = ack == AckOutcome.Positive;
        attempt.ApplyEvidence(ack, acceptance, positive ? AttemptResult.Succeeded : AttemptResult.FailedPermanent);
        State = positive ? DeliveryState.Succeeded : DeliveryState.FailedPermanent;
        StampUpdated(now, null);
    }

    /// <summary>Manual reconciliation of an Uncertain item (BR-DLV reconciliation): delivered, not delivered (resend) or failed.</summary>
    //public void Reconcile(
    //    ReconciliationResolution resolution,
    //    DateTimeOffset resendAt,
    //    Guid actorId,
    //    DateTimeOffset now
    //)
    //{
    //    if (State != DeliveryState.Uncertain)
    //        throw Invalid("Only an uncertain item can be reconciled.");
    //    Guard.NotEmpty(actorId, nameof(actorId));
    //    State = resolution switch
    //    {
    //        ReconciliationResolution.ConfirmedDelivered
    //        or ReconciliationResolution.LateAcknowledgment
    //        or ReconciliationResolution.StatusQuery => DeliveryState.Succeeded,
    //        ReconciliationResolution.ConfirmedNotDelivered or ReconciliationResolution.Resent =>
    //            DeliveryState.RetryScheduled,
    //        ReconciliationResolution.MarkedFailed => DeliveryState.FailedPermanent,
    //        _ => throw new ValidationException(
    //            "VALIDATION_INVALID_VALUE",
    //            "Invalid value",
    //            "Field 'resolution' contains an undefined value."
    //        ),
    //    };
    //    NextAttemptAt = State == DeliveryState.RetryScheduled ? resendAt : null;
    //    StampUpdated(now, actorId);
    //}

    public void Reconcile(Guid attemptId, ReconciliationResolution resolution, DateTimeOffset resendAt, Guid actorId, DateTimeOffset now)
    {
        if (State != DeliveryState.Uncertain)
            throw Invalid("Only an uncertain item can be reconciled.");
        Guard.NotEmpty(actorId, nameof(actorId));
        var attempt = FindAttempt(attemptId);

        var (next, result) = resolution switch
        {
            ReconciliationResolution.ConfirmedDelivered or ReconciliationResolution.LateAcknowledgment
                or ReconciliationResolution.StatusQuery => (DeliveryState.Succeeded, AttemptResult.Succeeded),
            ReconciliationResolution.ConfirmedNotDelivered or ReconciliationResolution.Resent
                => (DeliveryState.RetryScheduled, AttemptResult.FailedRetryable),
            ReconciliationResolution.MarkedFailed => (DeliveryState.FailedPermanent, AttemptResult.FailedPermanent),
            _ => throw new ValidationException("VALIDATION_INVALID_VALUE", "Invalid value",
                "Field 'resolution' contains an undefined value."),
        };

        attempt.ApplyEvidence(attempt.AckOutcome, attempt.AcceptanceOutcome, result);   // attempt no longer left Uncertain
        State = next;
        NextAttemptAt = next == DeliveryState.RetryScheduled ? resendAt : null;
        StampUpdated(now, actorId);
    }


    /// <summary>FailedPermanent -> Pending by an authorized manual retry; attempt counter restarts for policy purposes only via a new attempt number sequence (history is kept).</summary>
    public void ManualRetry(Guid actorId, DateTimeOffset now)
    {
        if (State != DeliveryState.FailedPermanent)
            throw Invalid("Only a permanently failed item can be retried manually.");
        State = DeliveryState.Pending;
        NextAttemptAt = null;
        StampUpdated(now, Guard.NotEmpty(actorId, nameof(actorId)));
    }

    /// <summary>Pending/RetryScheduled -> Quarantined (delivery quarantine); remembers the prior state for release.</summary>
    public void Quarantine(DateTimeOffset now)
    {
        if (State is not (DeliveryState.Pending or DeliveryState.RetryScheduled))
            throw Invalid("Only a Pending or RetryScheduled item can be quarantined.");
        QuarantinedFromState = State;
        State = DeliveryState.Quarantined;
        HoldReason = null;
        StampUpdated(now, null);
    }

    /// <summary>Quarantined -> Pending (release).</summary>
    //public void ReleaseFromQuarantine(Guid actorId, DateTimeOffset now)
    //{
    //    if (State != DeliveryState.Quarantined)
    //        throw Invalid("Only a quarantined item can be released.");
    //    State = DeliveryState.Pending;
    //    QuarantinedFromState = null;
    //    StampUpdated(now, Guard.NotEmpty(actorId, nameof(actorId)));
    //}

    public void ReleaseFromQuarantine(Guid actorId, DateTimeOffset now)
    {
        if (State != DeliveryState.Quarantined)
            throw Invalid("Only a quarantined item can be released.");
        State = QuarantinedFromState ?? DeliveryState.Pending;   // RetryScheduled keeps its NextAttemptAt
        QuarantinedFromState = null;
        StampUpdated(now, Guard.NotEmpty(actorId, nameof(actorId)));
    }

    public bool IsLeaseExpired(DateTimeOffset now) =>
        State == DeliveryState.InProgress && LeaseExpiresAt <= now;

    /// <summary>Maps the item state to the subject status requested from Module 7 (null = status stays as is).</summary>
    public static (OrderStatus Order, ResultStatus Result)? SubjectStatusFor(
        DeliveryState state,
        bool destinationRejected
    ) =>
        state switch
        {
            DeliveryState.InProgress => (OrderStatus.Delivering, ResultStatus.Delivering),
            DeliveryState.RetryScheduled => (
                OrderStatus.ReadyForDelivery,
                ResultStatus.ReadyForDelivery
            ),
            DeliveryState.FailedPermanent => destinationRejected
                ? (OrderStatus.DeliveryFailed, ResultStatus.DestinationRejected)
                : (OrderStatus.DeliveryFailed, ResultStatus.DeliveryFailed),
            DeliveryState.Uncertain => (
                OrderStatus.DeliveryUncertain,
                ResultStatus.DeliveryUncertain
            ),
            DeliveryState.Quarantined => (OrderStatus.Quarantined, ResultStatus.Quarantined),
            _ => null,
        };

    // Added
    private DeliveryAttempt FindAttempt(Guid attemptId) => _attempts.FirstOrDefault(a => a.Id == attemptId)
    ?? throw new ResourceNotFoundException("DELIVERY_ATTEMPT_NOT_FOUND", "Attempt not found",
        "The attempt does not belong to this delivery.");

    private static BusinessRuleException Invalid(string detail) =>
        new("DELIVERY_INVALID_STATE", "Invalid delivery state", detail);
}

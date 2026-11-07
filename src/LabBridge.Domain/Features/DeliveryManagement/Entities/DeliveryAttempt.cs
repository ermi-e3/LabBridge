using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.DeliveryManagement.Enums;

namespace LabBridge.Domain.Features.DeliveryManagement.Entities;

/// <summary>
/// One send attempt (append-only; never deleted). Outcome columns are written once at completion, except acknowledgment/acceptance updates by evidence.
/// ControlId is unique per organization and identifies the attempt on the wire. Error codes are sanitized (no payload, no secrets).
/// </summary>
public sealed class DeliveryAttempt : Entity, IOrganizationOwned
{
    private DeliveryAttempt() { }

    internal DeliveryAttempt(
        Guid organizationId,
        Guid deliveryId,
        int attemptNumber,
        string controlId,
        int connectorConfigVersion,
        DateTimeOffset startedAt
    )
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        DeliveryId = deliveryId;
        AttemptNumber = Guard.InRange(attemptNumber, 1, int.MaxValue, nameof(attemptNumber));
        ControlId = Guard.Required(controlId, nameof(controlId), 100);
        ConnectorConfigVersion = Guard.InRange(
            connectorConfigVersion,
            1,
            int.MaxValue,
            nameof(connectorConfigVersion)
        );
        TransportOutcome = TransportOutcome.NotAttempted;
        AckOutcome = AckOutcome.Pending;
        AcceptanceOutcome = AcceptanceOutcome.Pending;
        AttemptResult = AttemptResult.InProgress;
        StartedAt = startedAt;
    }

    public Guid OrganizationId { get; private set; }
    public Guid DeliveryId { get; private set; }
    public int AttemptNumber { get; private set; }
    public string ControlId { get; private set; } = null!;
    public TransportOutcome TransportOutcome { get; private set; }
    public AckOutcome AckOutcome { get; private set; }
    public AcceptanceOutcome AcceptanceOutcome { get; private set; }
    public AttemptResult AttemptResult { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorCategory { get; private set; }
    public int ConnectorConfigVersion { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    internal void Complete(
        TransportOutcome transport,
        AckOutcome ack,
        AcceptanceOutcome acceptance,
        AttemptResult result,
        string? errorCode,
        string? errorCategory,
        DateTimeOffset now
    )
    {
        if (AttemptResult != AttemptResult.InProgress)
            throw new BusinessRuleException(
                "ATTEMPT_ALREADY_COMPLETED",
                "Attempt already completed",
                "The attempt outcome is write-once."
            );
        if (result == AttemptResult.InProgress)
            throw new ValidationException(
                "ATTEMPT_RESULT_INVALID",
                "Invalid attempt result",
                "A completed attempt cannot remain in progress."
            );
        TransportOutcome = Guard.Defined(transport, nameof(transport));
        AckOutcome = Guard.Defined(ack, nameof(ack));
        AcceptanceOutcome = Guard.Defined(acceptance, nameof(acceptance));
        AttemptResult = Guard.Defined(result, nameof(result));
        ErrorCode = Guard.Optional(errorCode, nameof(errorCode), 50);
        ErrorCategory = Guard.Optional(errorCategory, nameof(errorCategory), 50);
        CompletedAt = now;
    }

    /// <summary>Late acknowledgment evidence for an attempt that ended Uncertain (the only permitted update after completion).</summary>
    //internal void ApplyEvidence(AckOutcome ack, AcceptanceOutcome acceptance, AttemptResult result)
    //{
    //    AckOutcome = Guard.Defined(ack, nameof(ack));
    //    AcceptanceOutcome = Guard.Defined(acceptance, nameof(acceptance));
    //    AttemptResult = Guard.Defined(result, nameof(result));
    //}

    internal void ApplyEvidence(AckOutcome ack, AcceptanceOutcome acceptance, AttemptResult result)
    {
        if (AttemptResult != AttemptResult.Uncertain)
            throw new BusinessRuleException("ATTEMPT_NOT_UNCERTAIN", "Attempt not uncertain",
                "Evidence can only be applied to an attempt that ended uncertain.");
        AckOutcome = Guard.Defined(ack, nameof(ack));
        AcceptanceOutcome = Guard.Defined(acceptance, nameof(acceptance));
        AttemptResult = Guard.Defined(result, nameof(result));
    }
}

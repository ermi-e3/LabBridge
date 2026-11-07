using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.DeliveryManagement.Enums;

namespace LabBridge.Domain.Features.DeliveryManagement.Entities;

/// <summary>Per-route retry/timeout/concurrency/circuit policy (key = route id). Without policy values there is no automatic retry (BR-DLV-008).</summary>
public sealed class RouteDeliveryPolicy : AggregateRoot, IOrganizationOwned
{
    private RouteDeliveryPolicy() { }

    public Guid OrganizationId { get; private set; }
    public Guid RouteId => Id;
    public int MaxAttempts { get; private set; }
    public int BaseDelaySeconds { get; private set; }
    public int MaxDelaySeconds { get; private set; }
    public JitterMode Jitter { get; private set; }
    public int SendTimeoutSeconds { get; private set; }
    public int AckTimeoutSeconds { get; private set; }
    public int ReconciliationEscalationMinutes { get; private set; }
    public int MaxConcurrency { get; private set; }
    public int? CircuitFailureThreshold { get; private set; }
    public int? CircuitPauseSeconds { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public static RouteDeliveryPolicy Define(
        Guid organizationId,
        Guid routeId,
        int maxAttempts,
        int baseDelaySeconds,
        int maxDelaySeconds,
        JitterMode jitter,
        int sendTimeoutSeconds,
        int ackTimeoutSeconds,
        int reconciliationEscalationMinutes,
        int maxConcurrency,
        int? circuitFailureThreshold,
        int? circuitPauseSeconds,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        var policy = new RouteDeliveryPolicy
        {
            Id = Guard.NotEmpty(routeId, nameof(routeId)),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
        };
        policy.Apply(
            maxAttempts,
            baseDelaySeconds,
            maxDelaySeconds,
            jitter,
            sendTimeoutSeconds,
            ackTimeoutSeconds,
            reconciliationEscalationMinutes,
            maxConcurrency,
            circuitFailureThreshold,
            circuitPauseSeconds,
            now,
            actorId
        );
        return policy;
    }

    public void Change(
        int maxAttempts,
        int baseDelaySeconds,
        int maxDelaySeconds,
        JitterMode jitter,
        int sendTimeoutSeconds,
        int ackTimeoutSeconds,
        int reconciliationEscalationMinutes,
        int maxConcurrency,
        int? circuitFailureThreshold,
        int? circuitPauseSeconds,
        DateTimeOffset now,
        Guid? actorId
    ) =>
        Apply(
            maxAttempts,
            baseDelaySeconds,
            maxDelaySeconds,
            jitter,
            sendTimeoutSeconds,
            ackTimeoutSeconds,
            reconciliationEscalationMinutes,
            maxConcurrency,
            circuitFailureThreshold,
            circuitPauseSeconds,
            now,
            actorId
        );

    /// <summary>Delay before the next attempt (exponential backoff capped at the maximum); jitter is applied by the caller with <paramref name="random"/> in [0,1).</summary>
    public TimeSpan NextDelay(int attemptsSoFar, double random)
    {
        var exponential = Math.Min(
            (double)MaxDelaySeconds,
            BaseDelaySeconds * Math.Pow(2, Math.Max(0, attemptsSoFar - 1))
        );
        var seconds = Jitter switch
        {
            JitterMode.Full => exponential * random,
            JitterMode.Equal => exponential / 2 + exponential / 2 * random,
            _ => exponential,
        };
        return TimeSpan.FromSeconds(seconds);
    }

    private void Apply(
        int maxAttempts,
        int baseDelay,
        int maxDelay,
        JitterMode jitter,
        int sendTimeout,
        int ackTimeout,
        int escalationMinutes,
        int maxConcurrency,
        int? threshold,
        int? pause,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        var validatedJitter = Guard.Defined(jitter, nameof(jitter));

        if (
            maxAttempts < 1
            || baseDelay < 0
            || maxDelay < baseDelay
            || sendTimeout <= 0
            || ackTimeout <= 0
            || escalationMinutes <= 0
            || maxConcurrency < 1
            || threshold is < 1
            || pause is <= 0
            || (threshold is null) != (pause is null)
        )
            throw new ValidationException(
                "DELIVERY_POLICY_INVALID",
                "Invalid delivery policy",
                "The delivery policy values are outside the permitted ranges."
            );
        MaxAttempts = maxAttempts;
        BaseDelaySeconds = baseDelay;
        MaxDelaySeconds = maxDelay;
        Jitter = Guard.Defined(jitter, nameof(jitter));
        SendTimeoutSeconds = sendTimeout;
        AckTimeoutSeconds = ackTimeout;
        ReconciliationEscalationMinutes = escalationMinutes;
        MaxConcurrency = maxConcurrency;
        CircuitFailureThreshold = threshold;
        CircuitPauseSeconds = pause;
        UpdatedAt = now;
        UpdatedBy = actorId;
    }
}

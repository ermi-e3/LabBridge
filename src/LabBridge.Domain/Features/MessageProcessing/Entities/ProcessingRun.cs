using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Enums;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.MessageProcessing.Enums;

namespace LabBridge.Domain.Features.MessageProcessing.Entities;

/// <summary>
/// One processing attempt for an inbound message. Never contains message content: error summaries are sanitized (BR-PRC-content rules, SC-PRC).
/// The database allows at most one open run (Scheduled/InProgress) per message.
/// </summary>
public sealed class ProcessingRun : AggregateRoot, IOrganizationOwned
{
    private ProcessingRun() { }

    public Guid OrganizationId { get; private set; }
    public Guid MessageId { get; private set; }
    public int AttemptNumber { get; private set; }
    public ProcessingTrigger Trigger { get; private set; }
    public ProcessingRunStatus Status { get; private set; }
    public ProcessingOutcome? Outcome { get; private set; }
    public string? ReasonCode { get; private set; }
    public MessageKind? MessageKind { get; private set; }
    public Guid ProfileVersionId { get; private set; }
    public Guid? BindingId { get; private set; }
    public Guid? ParserTypeId { get; private set; }
    public string? ParserVersion { get; private set; }
    public int? CanonicalSchemaVersion { get; private set; }
    public string? WorkerId { get; private set; }
    public DateTimeOffset? NextAttemptAt { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorSummary { get; private set; }
    public string? TraceId { get; private set; }

    public static ProcessingRun Schedule(
        Guid organizationId,
        Guid messageId,
        int attemptNumber,
        ProcessingTrigger trigger,
        Guid profileVersionId,
        DateTimeOffset nextAttemptAt
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
            MessageId = Guard.NotEmpty(messageId, nameof(messageId)),
            AttemptNumber = Guard.InRange(attemptNumber, 1, int.MaxValue, nameof(attemptNumber)),
            Trigger = Guard.Defined(trigger, nameof(trigger)),
            ProfileVersionId = Guard.NotEmpty(profileVersionId, nameof(profileVersionId)),
            Status = ProcessingRunStatus.Scheduled,
            NextAttemptAt = nextAttemptAt,
        };

    public void Start(
        string workerId,
        Guid? bindingId,
        ParserType? parserType,
        string? traceId,
        DateTimeOffset now
    )
    {
        Require(Status == ProcessingRunStatus.Scheduled, "Only a scheduled run can start.");
        Require(NextAttemptAt is null || NextAttemptAt <= now, "The scheduled time has not been reached.");
        WorkerId = Guard.Required(workerId, nameof(workerId), 100);
        BindingId = bindingId;
        ParserTypeId = parserType?.Id;
        ParserVersion = parserType?.Version;
        CanonicalSchemaVersion = parserType?.CanonicalSchemaVersion;
        TraceId = Guard.Optional(traceId, nameof(traceId), 64);
        StartedAt = now;
        Status = ProcessingRunStatus.InProgress;
    }

    public void Complete(
        ProcessingOutcome outcome,
        MessageKind? kind,
        string? reasonCode,
        string? errorCode,
        string? sanitizedErrorSummary,
        DateTimeOffset now
    )
    {
        Require(Status == ProcessingRunStatus.InProgress, "Only a run in progress can complete.");
        Outcome = Guard.Defined(outcome, nameof(outcome));
        MessageKind = kind;
        ReasonCode = Guard.Optional(reasonCode, nameof(reasonCode), 60);
        ErrorCode = Guard.Optional(errorCode, nameof(errorCode), 50);
        ErrorSummary = Guard.Optional(sanitizedErrorSummary, nameof(sanitizedErrorSummary), 500);
        NextAttemptAt = null;
        CompletedAt = now;
        Status = ProcessingRunStatus.Completed;
    }

    /// <summary>Abandons a Scheduled/InProgress run (lease loss, shutdown, superseded). Content-free reason.</summary>
    public void Abandon(string reasonCode, DateTimeOffset now)
    {
        Require(
            Status is ProcessingRunStatus.Scheduled or ProcessingRunStatus.InProgress,
            "Only an open run can be abandoned."
        );
        ReasonCode = Guard.Required(reasonCode, nameof(reasonCode), 60);
        NextAttemptAt = null;
        CompletedAt = now;
        Status = ProcessingRunStatus.Abandoned;
    }

    /// <summary>Frees a message blocked by a crashed worker (the open-run index counts InProgress as open).</summary>
    public bool AbandonIfStale(TimeSpan maxRuntime, DateTimeOffset now)
    {
        if (Status != ProcessingRunStatus.InProgress || StartedAt is null || now - StartedAt < maxRuntime)
            return false;
        Abandon("RUN_STALE", now);
        return true;
    }

    private static void Require(bool condition, string detail)
    {
        if (!condition)
            throw new BusinessRuleException(
                "PROCESSING_RUN_INVALID_STATE",
                "Invalid processing run state",
                detail
            );
    }
}

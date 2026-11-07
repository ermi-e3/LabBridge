using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.MessageProcessing.Enums;

namespace LabBridge.Domain.Features.MessageProcessing.Entities;

/// <summary>Append-only operator decision (reprocess or dismiss) on a failed or invalid run. A reason is mandatory.</summary>
public sealed class ProcessingReview : Entity, IOrganizationOwned
{
    private ProcessingReview() { }

    public Guid OrganizationId { get; private set; }
    public Guid MessageId { get; private set; }
    public Guid RunId { get; private set; }
    public ReviewDecision Decision { get; private set; }
    public string Reason { get; private set; } = null!;
    public Guid DecidedBy { get; private set; }
    public DateTimeOffset DecidedAt { get; private set; }

    public static ProcessingReview Decide(
        ProcessingRun reviewedRun,
        ReviewDecision decision,
        string reason,
        Guid decidedBy,
        DateTimeOffset now
    )
    {
        ArgumentNullException.ThrowIfNull(reviewedRun);
        if (
            reviewedRun.Status != ProcessingRunStatus.Completed
            || reviewedRun.Outcome is null or ProcessingOutcome.Normalized
        )
            throw new BusinessRuleException(
                "REVIEW_RUN_INVALID",
                "Run not reviewable",
                "Only a completed run that did not normalize can be reviewed."
            );
        return new ProcessingReview
        {
            Id = Guid.NewGuid(),
            OrganizationId = reviewedRun.OrganizationId,
            MessageId = reviewedRun.MessageId,
            RunId = reviewedRun.Id,
            Decision = Guard.Defined(decision, nameof(decision)),
            Reason = Guard.Required(reason, nameof(reason), 500),
            DecidedBy = Guard.NotEmpty(decidedBy, nameof(decidedBy)),
            DecidedAt = now,
        };
    }
}

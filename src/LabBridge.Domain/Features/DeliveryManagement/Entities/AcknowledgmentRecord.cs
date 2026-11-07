using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.DeliveryManagement.Enums;

namespace LabBridge.Domain.Features.DeliveryManagement.Entities;

/// <summary>An inbound acknowledgment message and its match to a delivery attempt (one per message). Content is never copied: only codes.</summary>
public sealed class AcknowledgmentRecord : AggregateRoot, IOrganizationOwned
{
    private AcknowledgmentRecord() { }

    public Guid OrganizationId { get; private set; }
    public Guid MessageId { get; private set; }
    public Guid? AttemptId { get; private set; }
    public string? AcknowledgesControlId { get; private set; }
    public string? AckCode { get; private set; }
    public string? AcceptanceCode { get; private set; }
    public string? ErrorCategory { get; private set; }
    public AckMatchStatus MatchStatus { get; private set; }
    public DateTimeOffset ReceivedAt { get; private set; }
    public DateTimeOffset? MatchedAt { get; private set; }

    public static AcknowledgmentRecord Receive(
        Guid organizationId,
        Guid messageId,
        string? acknowledgesControlId,
        string? ackCode,
        string? acceptanceCode,
        string? errorCategory,
        DateTimeOffset receivedAt
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
            MessageId = Guard.NotEmpty(messageId, nameof(messageId)),
            AcknowledgesControlId = Guard.Optional(
                acknowledgesControlId,
                nameof(acknowledgesControlId),
                100
            ),
            AckCode = Guard.Optional(ackCode, nameof(ackCode), 50),
            AcceptanceCode = Guard.Optional(acceptanceCode, nameof(acceptanceCode), 50),
            ErrorCategory = Guard.Optional(errorCategory, nameof(errorCategory), 50),
            MatchStatus = AckMatchStatus.Unmatched,
            ReceivedAt = receivedAt,
        };

    public void MatchTo(DeliveryAttempt attempt, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(attempt);
        if (MatchStatus == AckMatchStatus.Matched)
            throw new BusinessRuleException(
                "ACK_ALREADY_MATCHED",
                "Acknowledgment already matched",
                "The acknowledgment is already matched to an attempt."
            );
        if (attempt.OrganizationId != OrganizationId)
            throw new ForbiddenException(
                "TENANT_ACCESS_DENIED",
                "Tenant access denied",
                "The attempt does not belong to the organization."
            );
        AttemptId = attempt.Id;
        MatchStatus = AckMatchStatus.Matched;
        MatchedAt = now;
    }

    public void Ignore() =>
        MatchStatus =
            MatchStatus == AckMatchStatus.Matched
                ? throw new BusinessRuleException(
                    "ACK_ALREADY_MATCHED",
                    "Acknowledgment already matched",
                    "A matched acknowledgment cannot be ignored."
                )
                : AckMatchStatus.Ignored;
}

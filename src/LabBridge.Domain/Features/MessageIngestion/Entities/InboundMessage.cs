using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Enums;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Common.ValueObjects;
using LabBridge.Domain.Features.MessageIngestion.Enums;

namespace LabBridge.Domain.Features.MessageIngestion.Entities;

/// <summary>
/// An accepted, duplicate or quarantined inbound message (metadata; the original content lives in RawPayload).
/// Aggregate root of its status history. IsSimulated is copied from the connector and immutable (BR-ING-013).
/// Records the connector configuration version and profile version in force at receipt (BR-ING-014).
/// </summary>
public sealed class InboundMessage : AuditedAggregateRoot, IOrganizationOwned
{
    private readonly List<InboundMessageStatusHistory> _statusHistory = [];

    private InboundMessage() { }

    public Guid OrganizationId { get; private set; }
    public Guid ConnectorId { get; private set; }
    public int ConnectorConfigVersion { get; private set; }
    public Guid ProfileVersionId { get; private set; }
    public Guid CorrelationId { get; private set; }

    /// <summary>Strictly increasing sequence assigned by the database at insert (0 until persisted).</summary>
    public long ReceiptSequence { get; private set; }
    public DateTimeOffset ReceivedAt { get; private set; }
    public MessageStatus Status { get; private set; }
    public string? StatusReason { get; private set; }

    /// <summary>Set by Module 5 once the message kind is determined.</summary>
    public MessageKind? MessageKind { get; private set; }
    public bool IsSimulated { get; private set; }
    public string TransportType { get; private set; } = null!;

    /// <summary>Sanitized: no secrets.</summary>
    public string? RemoteEndpoint { get; private set; }
    public Guid PayloadId { get; private set; }
    public string ContentHash { get; private set; } = null!;
    public long SizeBytes { get; private set; }
    public string? EncodingHint { get; private set; }
    public string DedupKey { get; private set; } = null!;
    public DedupKeySource DedupKeySource { get; private set; }
    public Guid? OriginalMessageId { get; private set; }
    public ReceiptOutcome ReceiptOutcome { get; private set; }

    /// <summary>Adapter-defined reply descriptor (jsonb, at most 4 KB, never content).</summary>
    public string? ReceiptReplyDescriptor { get; private set; }
    public string? TraceId { get; private set; }

    public IReadOnlyCollection<InboundMessageStatusHistory> StatusHistory =>
        _statusHistory.AsReadOnly();

    /// <summary>
    /// Records receipt. Accepted -> Received; Duplicate -> Duplicate (linked to the original, payload shared); Quarantined -> Quarantined
    /// (conflicting duplicate, linked to the original). The caller persists the payload in the same transaction (BR-ING-001).
    /// </summary>
    public static InboundMessage Receive(
        Guid organizationId,
        Guid connectorId,
        int connectorConfigVersion,
        Guid profileVersionId,
        Guid correlationId,
        DateTimeOffset receivedAt,
        bool isSimulated,
        string transportType,
        string? remoteEndpoint,
        RawPayload payload,
        string dedupKey,
        DedupKeySource dedupKeySource,
        ReceiptOutcome outcome,
        Guid? originalMessageId,
        string? receiptReplyDescriptor,
        string? traceId,
        DateTimeOffset now
    )
    {
        ArgumentNullException.ThrowIfNull(payload);
        if (payload.OrganizationId != organizationId)
            throw new ForbiddenException(
                "TENANT_ACCESS_DENIED",
                "Tenant access denied",
                "The payload does not belong to the organization."
            );
        if (outcome != ReceiptOutcome.Accepted && originalMessageId is null)
            throw new ValidationException(
                "MESSAGE_ORIGINAL_REQUIRED",
                "Original message required",
                "Duplicates and conflicting duplicates must reference the original message."
            );
        if (receiptReplyDescriptor is { Length: > 4096 })
            throw new ValidationException(
                "MESSAGE_REPLY_DESCRIPTOR_TOO_LARGE",
                "Reply descriptor too large",
                "The reply descriptor must not exceed 4 KB."
            );

        var status = outcome switch
        {
            ReceiptOutcome.Accepted => MessageStatus.Received,
            ReceiptOutcome.Duplicate => MessageStatus.Duplicate,
            ReceiptOutcome.Quarantined => MessageStatus.Quarantined,
            _ => throw new ValidationException(
                "VALIDATION_INVALID_VALUE",
                "Invalid value",
                "Field 'outcome' contains an undefined value."
            ),
        };
        var message = new InboundMessage
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            ConnectorId = Guard.NotEmpty(connectorId, nameof(connectorId)),
            ConnectorConfigVersion = Guard.InRange(
                connectorConfigVersion,
                1,
                int.MaxValue,
                nameof(connectorConfigVersion)
            ),
            ProfileVersionId = Guard.NotEmpty(profileVersionId, nameof(profileVersionId)),
            CorrelationId = Guard.NotEmpty(correlationId, nameof(correlationId)),
            ReceivedAt = receivedAt,
            Status = status,
            StatusReason = outcome == ReceiptOutcome.Quarantined ? "ConflictingDuplicate" : null,
            IsSimulated = isSimulated,
            TransportType = Guard.Required(transportType, nameof(transportType), 50),
            RemoteEndpoint = Guard.Optional(remoteEndpoint, nameof(remoteEndpoint), 300),
            PayloadId = payload.Id,
            ContentHash = payload.ContentHash,
            SizeBytes = payload.SizeBytes,
            EncodingHint = payload.EncodingHint,
            DedupKey = Guard.Required(dedupKey, nameof(dedupKey), 128),
            DedupKeySource = Guard.Defined(dedupKeySource, nameof(dedupKeySource)),
            OriginalMessageId = originalMessageId,
            ReceiptOutcome = outcome,
            ReceiptReplyDescriptor = receiptReplyDescriptor,
            TraceId = Guard.Optional(traceId, nameof(traceId), 64),
        };
        message.StampCreated(now, null);
        message._statusHistory.Add(
            new InboundMessageStatusHistory(
                organizationId,
                message.Id,
                null,
                status,
                message.StatusReason,
                ActorType.System,
                null,
                correlationId,
                now
            )
        );
        message.Raise(
            new InboundMessageReceived(organizationId, message.Id, connectorId, outcome, now)
        );
        return message;
    }

    /// <summary>Applies a status transition permitted by <see cref="MessageStatusTransitions"/> and appends history (BR-ING-018).</summary>
    public void TransitionTo(
        MessageStatus to,
        string? reason,
        ActorType actorType,
        Guid? actorId,
        Guid? correlationId,
        DateTimeOffset now
    )
    {
        if (!MessageStatusTransitions.IsAllowed(Status, to))
            throw new BusinessRuleException(
                "MESSAGE_INVALID_STATE",
                "Invalid message state",
                $"A message in status {Status} cannot move to {to}."
            );
        if (to == MessageStatus.Duplicate && OriginalMessageId is null)
            throw new BusinessRuleException(
                "MESSAGE_DUPLICATE_WITHOUT_ORIGINAL",
                "Original message required",
                "A message can only become a duplicate when it references an original."
            );
        var from = Status;
        Status = to;
        StatusReason = Guard.Optional(reason, nameof(reason), 500);
        StampUpdated(now, actorType == ActorType.User ? actorId : null);
        _statusHistory.Add(
            new InboundMessageStatusHistory(
                OrganizationId,
                Id,
                from,
                to,
                StatusReason,
                actorType,
                actorId,
                correlationId,
                now
            )
        );
    }

    public void AssignMessageKind(MessageKind kind, DateTimeOffset now)
    {
        MessageKind = Guard.Defined(kind, nameof(kind));
        StampUpdated(now, null);
    }

    /// <summary>Quarantined -> Received by an authorized user; the message is processed as new content. A reason is mandatory (BR-ING-021).</summary>
    public void ReleaseFromQuarantine(
        string reason,
        Guid actorId,
        Guid? correlationId,
        DateTimeOffset now
    )
    {
        EnsureQuarantined();
        TransitionTo(
            MessageStatus.Received,
            Guard.Required(reason, nameof(reason), 500),
            ActorType.User,
            actorId,
            correlationId,
            now
        );
    }

    /// <summary>Quarantined -> Duplicate by an authorized user. A reason is mandatory (BR-ING-021).</summary>
    public void ConfirmDuplicate(
        string reason,
        Guid actorId,
        Guid? correlationId,
        DateTimeOffset now
    )
    {
        EnsureQuarantined();
        TransitionTo(
            MessageStatus.Duplicate,
            Guard.Required(reason, nameof(reason), 500),
            ActorType.User,
            actorId,
            correlationId,
            now
        );
    }

    private void EnsureQuarantined()
    {
        if (Status != MessageStatus.Quarantined)
            throw new BusinessRuleException(
                "MESSAGE_INVALID_STATE",
                "Invalid message state",
                "Only a quarantined message can be resolved."
            );
    }
}

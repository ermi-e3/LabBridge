using LabBridge.Domain.Common;
using LabBridge.Domain.Common.ValueObjects;
using LabBridge.Domain.Features.MessageIngestion.Enums;

namespace LabBridge.Domain.Features.MessageIngestion.Entities;

/// <summary>Append-only status history; never rewritten (BR-ING-018).</summary>
public sealed class InboundMessageStatusHistory : Entity, IOrganizationOwned
{
    private InboundMessageStatusHistory() { }

    internal InboundMessageStatusHistory(
        Guid organizationId,
        Guid messageId,
        MessageStatus? from,
        MessageStatus to,
        string? reason,
        ActorType actorType,
        Guid? actorId,
        Guid? correlationId,
        DateTimeOffset changedAt
    )
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        MessageId = messageId;
        FromStatus = from;
        ToStatus = to;
        Reason = Guard.Optional(reason, nameof(reason), 500);
        ActorType = actorType;
        ActorId = actorId;
        CorrelationId = correlationId;
        ChangedAt = changedAt;
    }

    public Guid OrganizationId { get; private set; }
    public Guid MessageId { get; private set; }
    public MessageStatus? FromStatus { get; private set; }
    public MessageStatus ToStatus { get; private set; }
    public string? Reason { get; private set; }
    public ActorType ActorType { get; private set; }
    public Guid? ActorId { get; private set; }
    public Guid? CorrelationId { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
}

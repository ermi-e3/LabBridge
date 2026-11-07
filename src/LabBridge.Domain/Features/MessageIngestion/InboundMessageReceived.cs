using LabBridge.Domain.Common.Events;
using LabBridge.Domain.Features.MessageIngestion.Enums;

namespace LabBridge.Domain.Features.MessageIngestion;

/// <summary>Raised when a message has been received and preserved (positive receipt means only "received and preserved", BR-ING-004).</summary>
public sealed record InboundMessageReceived(
    Guid OrganizationId,
    Guid MessageId,
    Guid ConnectorId,
    ReceiptOutcome Outcome,
    DateTimeOffset OccurredAt
) : DomainEvent(OccurredAt);

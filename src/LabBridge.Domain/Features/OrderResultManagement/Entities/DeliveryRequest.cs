using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.OrderResultManagement.Enums;

namespace LabBridge.Domain.Features.OrderResultManagement.Entities;

/// <summary>
/// Persistent, lease-based request asking Module 8 to deliver an order or result for a route and payload version (1 in the MVP).
/// Created in the same transaction as the ReadyForDelivery transition; one per (subject, payload version). The subject reference is polymorphic
/// (validated by the application).
/// </summary>
public sealed class DeliveryRequest : AuditedAggregateRoot, IOrganizationOwned
{
    private DeliveryRequest() { }

    public Guid OrganizationId { get; private set; }
    public DeliverySubjectType SubjectType { get; private set; }
    public Guid SubjectId { get; private set; }
    public Guid RouteId { get; private set; }
    public int PayloadVersion { get; private set; }
    public DeliveryRequestStatus Status { get; private set; }
    public long ReceiptSequence { get; private set; }
    public DateTimeOffset AvailableAt { get; private set; }
    public string? ClaimedBy { get; private set; }
    public DateTimeOffset? LeaseExpiresAt { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public static DeliveryRequest Create(
        Guid organizationId,
        DeliverySubjectType subjectType,
        Guid subjectId,
        Guid routeId,
        int payloadVersion,
        long receiptSequence,
        DateTimeOffset now
    )
    {
        if (receiptSequence <= 0)
            throw new ValidationException(
                "DELIVERY_SEQUENCE_REQUIRED",
                "Receipt sequence required",
                "The receipt sequence of the source message is required."
            );
        var request = new DeliveryRequest
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
            SubjectType = Guard.Defined(subjectType, nameof(subjectType)),
            SubjectId = Guard.NotEmpty(subjectId, nameof(subjectId)),
            RouteId = Guard.NotEmpty(routeId, nameof(routeId)),
            PayloadVersion = Guard.InRange(payloadVersion, 1, int.MaxValue, nameof(payloadVersion)),
            ReceiptSequence = receiptSequence,
            Status = DeliveryRequestStatus.Pending,
            AvailableAt = now,
        };
        request.StampCreated(now, null);
        return request;
    }

    public void Claim(string claimedBy, DateTimeOffset leaseExpiresAt, DateTimeOffset now)
    {
        if (Status != DeliveryRequestStatus.Pending || AvailableAt > now)
            throw Invalid("Only an available pending request can be claimed.");
        if (leaseExpiresAt <= now)
            throw new ValidationException(
                "DELIVERY_LEASE_INVALID",
                "Invalid lease",
                "The lease must expire in the future."
            );
        ClaimedBy = Guard.Required(claimedBy, nameof(claimedBy), 100);
        LeaseExpiresAt = leaseExpiresAt;
        AttemptCount++;
        Status = DeliveryRequestStatus.Claimed;
        StampUpdated(now, null);
    }

    public void ReleaseClaim(DateTimeOffset now)
    {
        if (Status != DeliveryRequestStatus.Claimed)
            throw Invalid("Only a claimed request can be released.");
        Clear();
        Status = DeliveryRequestStatus.Pending;
        AvailableAt = now;
        StampUpdated(now, null);
    }

    public void Complete(DateTimeOffset now)
    {
        if (Status != DeliveryRequestStatus.Claimed)
            throw Invalid("Only a claimed request can be completed.");
        Clear();
        Status = DeliveryRequestStatus.Completed;
        CompletedAt = now;
        StampUpdated(now, null);
    }

    public void Cancel(DateTimeOffset now)
    {
        if (Status == DeliveryRequestStatus.Completed)
            throw Invalid("A completed request cannot be cancelled.");
        if (Status == DeliveryRequestStatus.Cancelled)
            return;
        Clear();
        Status = DeliveryRequestStatus.Cancelled;
        StampUpdated(now, null);
    }

    private void Clear()
    {
        ClaimedBy = null;
        LeaseExpiresAt = null;
    }

    private static BusinessRuleException Invalid(string detail) =>
        new("DELIVERY_REQUEST_INVALID_STATE", "Invalid delivery request state", detail);
}

using LabBridge.Domain.Common.Exceptions;

namespace LabBridge.Domain.Common.Idempotency;

/// <summary>
/// Stored outcome of an idempotent API call (Stage 1 section 11.4): key, request fingerprint, response status/body, expiry.
/// ASSUMPTION A-KERNEL-01 (DEC-033 open): ownership is unassigned in the FRS; it is modelled in the shared kernel because several modules use it and
/// no module owns it. OrganizationId is null for platform-scoped calls. Same key + same fingerprint replays the response; a different fingerprint
/// is a conflict (LB-SYS-409-IDEMPOTENCY-MISMATCH). The retention window is DEC-017 (open) and supplied by the caller.
/// </summary>
//public sealed class IdempotencyRecord : Entity
//{
//    private IdempotencyRecord() { }

//    public Guid? OrganizationId { get; private set; }
//    public string OperationScope { get; private set; } = null!;
//    public string Key { get; private set; } = null!;
//    public string Fingerprint { get; private set; } = null!;
//    public int ResponseStatus { get; private set; }
//    public string? ResponseBody { get; private set; }
//    public DateTimeOffset CreatedAt { get; private set; }
//    public DateTimeOffset ExpiresAt { get; private set; }

//    public static IdempotencyRecord Store(Guid? organizationId, string key, string fingerprint, int responseStatus, string? responseBody,
//        DateTimeOffset now, TimeSpan retention)
//    {
//        if (key is null || key.Length is < 16 or > 128)
//            throw new ValidationException("IDEMPOTENCY_KEY_INVALID", "Invalid idempotency key", "The idempotency key must be 16-128 characters.");
//        if (retention <= TimeSpan.Zero)
//            throw new ValidationException("IDEMPOTENCY_RETENTION_INVALID", "Invalid retention", "The retention window must be positive.");
//        return new IdempotencyRecord
//        {
//            Id = Guid.NewGuid(),
//            OrganizationId = Guard.NotEmptyWhenSet(organizationId, nameof(organizationId)),
//            Key = key,
//            Fingerprint = Guard.Required(fingerprint, nameof(fingerprint), 128),
//            ResponseStatus = Guard.InRange(responseStatus, 100, 599, nameof(responseStatus)),
//            ResponseBody = responseBody,
//            CreatedAt = now,
//            ExpiresAt = now + retention
//        };
//    }

//    public bool IsExpired(DateTimeOffset now) => ExpiresAt <= now;

//    /// <summary>Same fingerprint: the stored response is replayed. Different fingerprint: conflict.</summary>
//    public void EnsureSameRequest(string fingerprint)
//    {
//        if (!string.Equals(Fingerprint, fingerprint, StringComparison.Ordinal))
//            throw new ConflictException("IDEMPOTENCY_MISMATCH", "Idempotency key reused", "The idempotency key was used with a different request.");
//    }
//}



public enum IdempotencyStatus
{
    InProgress = 1,
    Completed = 2
}

public sealed class IdempotencyRecord : Entity
{
    private IdempotencyRecord() { }

    public Guid? OrganizationId { get; private set; }

    public string OperationScope { get; private set; } = null!;

    public string Key { get; private set; } = null!;

    public string Fingerprint { get; private set; } = null!;

    public IdempotencyStatus Status { get; private set; }

    /// <summary>
    /// Identifies the current owner of a reservation generation.
    /// Changes whenever an expired reservation is reclaimed.
    /// </summary>
    public Guid ReservationToken { get; private set; }

    public int? ResponseStatus { get; private set; }

    public string? ResponseBody { get; private set; }

    public string? ResponseContentType { get; private set; }

    public string? ResponseHeaders { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public static IdempotencyRecord Reserve(
        Guid? organizationId,
        string operationScope,
        string key,
        string fingerprint,
        DateTimeOffset now,
        TimeSpan retention)
    {
        if (organizationId == Guid.Empty)
            throw new ValidationException(
                "IDEMPOTENCY_ORGANIZATION_INVALID",
                "Invalid organization",
                "Organization ID cannot be an empty GUID.");

        var scope = Guard.Required(operationScope, nameof(operationScope), 256);
        var idempotencyKey = Guard.RequiredExact(key, nameof(key), 128);
        var requestFingerprint = Guard.RequiredExact(
            fingerprint,
            nameof(fingerprint),
            128);

        if (idempotencyKey.Length < 16)
            throw new ValidationException(
                "IDEMPOTENCY_KEY_INVALID",
                "Invalid idempotency key",
                "The idempotency key must contain at least 16 characters.");

        if (retention <= TimeSpan.Zero)
            throw new ValidationException(
                "IDEMPOTENCY_RETENTION_INVALID",
                "Invalid retention period",
                "The idempotency retention period must be greater than zero.");

        DateTimeOffset expiresAt;

        try
        {
            expiresAt = now.Add(retention);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new ValidationException(
                "IDEMPOTENCY_RETENTION_INVALID",
                "Invalid retention period",
                "The idempotency expiration date is outside the supported range.");
        }

        return new IdempotencyRecord
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            OperationScope = scope,
            Key = idempotencyKey,
            Fingerprint = requestFingerprint,
            Status = IdempotencyStatus.InProgress,
            ReservationToken = Guid.NewGuid(),
            CreatedAt = now,
            ExpiresAt = expiresAt
        };
    }

    public void Complete(
        int responseStatus,
        string? responseBody,
        string? contentType,
        string? headers)
    {
        if (Status != IdempotencyStatus.InProgress)
            throw new ConflictException(
                "IDEMPOTENCY_ALREADY_COMPLETED",
                "Idempotency record already completed",
                "Only an in-progress idempotency reservation can be completed.");

        if (responseStatus is < 200 or > 299)
            throw new ValidationException(
                "IDEMPOTENCY_RESPONSE_STATUS_INVALID",
                "Invalid response status",
                "Only successful HTTP responses can be stored.");

        ResponseBody = responseBody;
        ResponseContentType = Guard.Optional(contentType, nameof(contentType), 128);
        ResponseHeaders = Guard.OptionalJson(headers, nameof(headers));
        ResponseStatus = responseStatus;
        Status = IdempotencyStatus.Completed;
    }

    public bool IsExpired(DateTimeOffset now)
        => ExpiresAt <= now;

    public void EnsureSameRequest(string fingerprint)
    {
        if (!string.Equals(
                Fingerprint,
                fingerprint,
                StringComparison.Ordinal))
        {
            throw new ConflictException(
                "IDEMPOTENCY_MISMATCH",
                "Idempotency key reused",
                "The idempotency key was used with a different request.");
        }
    }

    public void ReReserve(IdempotencyRecord newReservation)
    {
        ArgumentNullException.ThrowIfNull(newReservation);

        if (!IsExpired(newReservation.CreatedAt))
            throw new ConflictException(
                "IDEMPOTENCY_NOT_EXPIRED",
                "Reservation cannot be reclaimed",
                "The existing idempotency record has not expired.");

        if (OrganizationId != newReservation.OrganizationId ||
            !string.Equals(
                OperationScope,
                newReservation.OperationScope,
                StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(
                Key,
                newReservation.Key,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException(
                "IDEMPOTENCY_RESERVATION_MISMATCH",
                "Reservation mismatch",
                "The new reservation does not belong to the existing idempotency record.");
        }

        Fingerprint = newReservation.Fingerprint;
        Status = IdempotencyStatus.InProgress;
        ReservationToken = newReservation.ReservationToken;

        ResponseStatus = null;
        ResponseBody = null;
        ResponseContentType = null;
        ResponseHeaders = null;

        CreatedAt = newReservation.CreatedAt;
        ExpiresAt = newReservation.ExpiresAt;
    }
}
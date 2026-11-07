using LabBridge.Domain.Common.Exceptions;

namespace LabBridge.Domain.Common.Idempotency;

/// <summary>
/// Stored outcome of an idempotent API call (Stage 1 section 11.4): key, request fingerprint, response status/body, expiry.
/// ASSUMPTION A-KERNEL-01 (DEC-033 open): ownership is unassigned in the FRS; it is modelled in the shared kernel because several modules use it and
/// no module owns it. OrganizationId is null for platform-scoped calls. Same key + same fingerprint replays the response; a different fingerprint
/// is a conflict (LB-SYS-409-IDEMPOTENCY-MISMATCH). The retention window is DEC-017 (open) and supplied by the caller.
/// </summary>
public sealed class IdempotencyRecord : Entity
{
    private IdempotencyRecord() { }

    public Guid? OrganizationId { get; private set; }
    public string Key { get; private set; } = null!;
    public string Fingerprint { get; private set; } = null!;
    public int ResponseStatus { get; private set; }
    public string? ResponseBody { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    public static IdempotencyRecord Store(Guid? organizationId, string key, string fingerprint, int responseStatus, string? responseBody,
        DateTimeOffset now, TimeSpan retention)
    {
        if (key is null || key.Length is < 16 or > 128)
            throw new ValidationException("IDEMPOTENCY_KEY_INVALID", "Invalid idempotency key", "The idempotency key must be 16-128 characters.");
        if (retention <= TimeSpan.Zero)
            throw new ValidationException("IDEMPOTENCY_RETENTION_INVALID", "Invalid retention", "The retention window must be positive.");
        return new IdempotencyRecord
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmptyWhenSet(organizationId, nameof(organizationId)),
            Key = key,
            Fingerprint = Guard.Required(fingerprint, nameof(fingerprint), 128),
            ResponseStatus = Guard.InRange(responseStatus, 100, 599, nameof(responseStatus)),
            ResponseBody = responseBody,
            CreatedAt = now,
            ExpiresAt = now + retention
        };
    }

    public bool IsExpired(DateTimeOffset now) => ExpiresAt <= now;

    /// <summary>Same fingerprint: the stored response is replayed. Different fingerprint: conflict.</summary>
    public void EnsureSameRequest(string fingerprint)
    {
        if (!string.Equals(Fingerprint, fingerprint, StringComparison.Ordinal))
            throw new ConflictException("IDEMPOTENCY_MISMATCH", "Idempotency key reused", "The idempotency key was used with a different request.");
    }
}

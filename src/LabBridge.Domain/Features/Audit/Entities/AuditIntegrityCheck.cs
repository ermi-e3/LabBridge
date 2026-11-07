using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.Audit.Enums;

namespace LabBridge.Domain.Features.Audit.Entities;

/// <summary>A requested verification of the seal chain and event hashes over a time range of a stream (retained per DEC-008).</summary>
public sealed class AuditIntegrityCheck : Entity
{
    private AuditIntegrityCheck() { }

    public Guid? OrganizationId { get; private set; }
    public AuditScope Stream { get; private set; }
    public DateTimeOffset RangeFrom { get; private set; }
    public DateTimeOffset RangeTo { get; private set; }
    public IntegrityCheckStatus Status { get; private set; }
    public string? FirstBreakJson { get; private set; }
    public Guid RequestedBy { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public static AuditIntegrityCheck Start(
        Guid? organizationId,
        DateTimeOffset rangeFrom,
        DateTimeOffset rangeTo,
        Guid requestedBy,
        DateTimeOffset now
    )
    {
        if (rangeTo <= rangeFrom)
            throw new ValidationException(
                "AUDIT_RANGE_INVALID",
                "Invalid range",
                "The range end must be after the range start."
            );
        return new AuditIntegrityCheck
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmptyWhenSet(organizationId, nameof(organizationId)),
            Stream = organizationId is null ? AuditScope.Platform : AuditScope.Organization,
            RangeFrom = rangeFrom,
            RangeTo = rangeTo,
            Status = IntegrityCheckStatus.Running,
            RequestedBy = Guard.NotEmpty(requestedBy, nameof(requestedBy)),
            StartedAt = now,
        };
    }

    public void Complete(bool passed, string? firstBreakJson, DateTimeOffset now)
    {
        if (Status != IntegrityCheckStatus.Running)
            throw new BusinessRuleException(
                "AUDIT_CHECK_INVALID_STATE",
                "Invalid check state",
                "The integrity check has already completed."
            );
        if (!passed && firstBreakJson is null)
            throw new ValidationException(
                "AUDIT_BREAK_REQUIRED",
                "Break details required",
                "A failed check must describe the first break."
            );
        Status = passed ? IntegrityCheckStatus.Passed : IntegrityCheckStatus.Failed;
        FirstBreakJson = firstBreakJson;
        CompletedAt = now;
    }
}

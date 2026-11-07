using LabBridge.Domain.Common;
using LabBridge.Domain.Features.OrganizationManagement.Enums;

namespace LabBridge.Domain.Features.OrganizationManagement.Entities;

/// <summary>Insert-only record of an organization status change (BR-ORG-009). (OrganizationId, StatusVersion) is unique.</summary>
public sealed class OrganizationStatusHistory : Entity, IOrganizationOwned
{
    private OrganizationStatusHistory() { }

    internal OrganizationStatusHistory(
        Guid organizationId,
        OrganizationStatus? from,
        OrganizationStatus to,
        SuspensionMode? mode,
        string? reason,
        int statusVersion,
        DateTimeOffset changedAt,
        Guid? changedBy,
        Guid? correlationId
    )
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        FromStatus = from;
        ToStatus = to;
        SuspensionMode = mode;
        Reason = reason;
        StatusVersion = statusVersion;
        ChangedAt = changedAt;
        ChangedBy = changedBy;
        CorrelationId = correlationId;
    }

    public Guid OrganizationId { get; private set; }
    public OrganizationStatus? FromStatus { get; private set; }
    public OrganizationStatus ToStatus { get; private set; }
    public SuspensionMode? SuspensionMode { get; private set; }
    public string? Reason { get; private set; }
    public int StatusVersion { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
    public Guid? ChangedBy { get; private set; }
    public Guid? CorrelationId { get; private set; }
}

using LabBridge.Domain.Common.Events;
using LabBridge.Domain.Features.OrganizationManagement.Enums;

namespace LabBridge.Domain.Features.OrganizationManagement;

/// <summary>Raised on every organization status change; downstream modules react (for example hold/release of inbound processing).</summary>
public sealed record OrganizationStatusChanged(
    Guid OrganizationId,
    OrganizationStatus? FromStatus,
    OrganizationStatus ToStatus,
    SuspensionMode? SuspensionMode,
    int StatusVersion,
    DateTimeOffset OccurredAt
) : DomainEvent(OccurredAt);

using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.TestCodeMapping.Enums;

namespace LabBridge.Domain.Features.TestCodeMapping.Entities;

/// <summary>Immutable record of one mapping execution for an order or result (subject identifier from Module 7, no cross-module FK). RequestId makes it idempotent.</summary>
public sealed class MappingRun : AggregateRoot, IOrganizationOwned
{
    private MappingRun() { }

    public Guid OrganizationId { get; private set; }
    public MappingSubjectType SubjectType { get; private set; }
    public Guid SubjectId { get; private set; }
    public Guid RequestId { get; private set; }
    public Guid RouteId { get; private set; }
    public MappingRunTrigger Trigger { get; private set; }
    public MappingRunStatus Status { get; private set; }
    public int ResolutionCount { get; private set; }
    public int FailureCount { get; private set; }
    public bool IsSimulated { get; private set; }
    public Guid? RequestedBy { get; private set; }
    public Guid? CorrelationId { get; private set; }
    public string? TraceId { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset CompletedAt { get; private set; }

    public static MappingRun Record(
        Guid organizationId,
        MappingSubjectType subjectType,
        Guid subjectId,
        Guid requestId,
        Guid routeId,
        MappingRunTrigger trigger,
        int resolutionCount,
        int failureCount,
        bool isSimulated,
        Guid? requestedBy,
        Guid? correlationId,
        string? traceId,
        DateTimeOffset startedAt,
        DateTimeOffset completedAt
    )
    {
        if (failureCount < 0 || resolutionCount < failureCount)
            throw new ValidationException(
                "MAPPING_RUN_COUNTS_INVALID",
                "Invalid run counts",
                "Failure count must be between 0 and the resolution count."
            );
        return new MappingRun
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
            SubjectType = Guard.Defined(subjectType, nameof(subjectType)),
            SubjectId = Guard.NotEmpty(subjectId, nameof(subjectId)),
            RequestId = Guard.NotEmpty(requestId, nameof(requestId)),
            RouteId = Guard.NotEmpty(routeId, nameof(routeId)),
            Trigger = Guard.Defined(trigger, nameof(trigger)),
            Status = failureCount == 0 ? MappingRunStatus.Succeeded : MappingRunStatus.Failed,
            ResolutionCount = resolutionCount,
            FailureCount = failureCount,
            IsSimulated = isSimulated,
            RequestedBy = requestedBy,
            CorrelationId = correlationId,
            TraceId = Guard.Optional(traceId, nameof(traceId), 64),
            StartedAt = startedAt,
            CompletedAt = completedAt,
        };
    }
}

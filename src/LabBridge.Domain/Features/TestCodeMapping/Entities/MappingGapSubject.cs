using LabBridge.Domain.Common;
using LabBridge.Domain.Features.TestCodeMapping.Enums;

namespace LabBridge.Domain.Features.TestCodeMapping.Entities;

/// <summary>Link between a gap and an affected subject (order/result). Composite key (GapId, SubjectType, SubjectId); insert-only.</summary>
public sealed class MappingGapSubject : IOrganizationOwned
{
    private MappingGapSubject() { }

    internal MappingGapSubject(
        MappingGap gap,
        MappingSubjectType type,
        Guid subjectId,
        Guid? runId,
        DateTimeOffset now
    )
    {
        OrganizationId = gap.OrganizationId;
        GapId = gap.Id;
        SubjectType = Guard.Defined(type, nameof(type));
        SubjectId = Guard.NotEmpty(subjectId, nameof(subjectId));
        RunId = runId;
        FirstSeenAt = now;
    }

    public Guid OrganizationId { get; private set; }
    public Guid GapId { get; private set; }
    public MappingSubjectType SubjectType { get; private set; }
    public Guid SubjectId { get; private set; }
    public Guid? RunId { get; private set; }
    public DateTimeOffset FirstSeenAt { get; private set; }
}

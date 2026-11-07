using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.TestCodeMapping.Enums;

namespace LabBridge.Domain.Features.TestCodeMapping.Entities;

/// <summary>
/// An unmapped code / unconfigured mapping surfaced to operators. Deduplicated while Open or Ignored (one row per key); occurrences are counted,
/// affected subjects are linked through MappingGapSubject. A Resolved gap that recurs opens a new row.
/// </summary>
public sealed class MappingGap : AggregateRoot, IOrganizationOwned
{
    private MappingGap() { }

    public Guid OrganizationId { get; private set; }
    public Guid? MappingSetId { get; private set; }
    public CounterpartyKind CounterpartyKind { get; private set; }
    public Guid CounterpartyId { get; private set; }
    public MappingDirection Direction { get; private set; }
    public MappingDomain Domain { get; private set; }
    public MappingGapKind GapKind { get; private set; }
    public string? Code { get; private set; }
    public string? CodeSystem { get; private set; }
    public GapStatus Status { get; private set; }
    public int OccurrenceCount { get; private set; }
    public DateTimeOffset FirstSeenAt { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }
    public Guid? ResolvedByVersionId { get; private set; }
    public string? IgnoredReason { get; private set; }
    public Guid? IgnoredBy { get; private set; }

    public static MappingGap Open(
        Guid organizationId,
        Guid? mappingSetId,
        CounterpartyKind kind,
        Guid counterpartyId,
        MappingDirection direction,
        MappingDomain domain,
        MappingGapKind gapKind,
        string? code,
        string? codeSystem,
        DateTimeOffset now
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
            MappingSetId = mappingSetId,
            CounterpartyKind = Guard.Defined(kind, nameof(kind)),
            CounterpartyId = Guard.NotEmpty(counterpartyId, nameof(counterpartyId)),
            Direction = Guard.Defined(direction, nameof(direction)),
            Domain = Guard.Defined(domain, nameof(domain)),
            GapKind = Guard.Defined(gapKind, nameof(gapKind)),
            Code = Guard.Optional(code, nameof(code), 100),
            CodeSystem = Guard.Optional(codeSystem, nameof(codeSystem), 100),
            Status = GapStatus.Open,
            OccurrenceCount = 1,
            FirstSeenAt = now,
            LastSeenAt = now,
        };

    public void RecordOccurrence(DateTimeOffset now)
    {
        if (Status == GapStatus.Resolved)
            throw new BusinessRuleException(
                "MAPPING_GAP_RESOLVED",
                "Gap resolved",
                "A resolved gap cannot receive occurrences; a new gap must be opened."
            );
        OccurrenceCount++;
        if (now > LastSeenAt)
            LastSeenAt = now;
    }

    public MappingGapSubject LinkSubject(
        MappingSubjectType type,
        Guid subjectId,
        Guid? runId,
        DateTimeOffset now
    ) => new(this, type, subjectId, runId, now);

    public void Resolve(Guid resolvedByVersionId, DateTimeOffset now)
    {
        if (Status == GapStatus.Resolved)
            return;
        Status = GapStatus.Resolved;
        ResolvedAt = now;
        ResolvedByVersionId = Guard.NotEmpty(resolvedByVersionId, nameof(resolvedByVersionId));
    }

    public void Ignore(string reason, Guid actorId)
    {
        if (Status != GapStatus.Open)
            throw new BusinessRuleException(
                "MAPPING_GAP_INVALID_STATE",
                "Invalid gap state",
                "Only an open gap can be ignored."
            );
        IgnoredReason = Guard.Required(reason, nameof(reason), 500);
        IgnoredBy = Guard.NotEmpty(actorId, nameof(actorId));
        Status = GapStatus.Ignored;
    }

    public void Reopen()
    {
        if (Status != GapStatus.Ignored)
            throw new BusinessRuleException(
                "MAPPING_GAP_INVALID_STATE",
                "Invalid gap state",
                "Only an ignored gap can be reopened."
            );
        IgnoredReason = null;
        IgnoredBy = null;
        Status = GapStatus.Open;
    }
}

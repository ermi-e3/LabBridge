using LabBridge.Domain.Common;
using LabBridge.Domain.Features.MessageProcessing.Enums;

namespace LabBridge.Domain.Features.MessageProcessing.Entities;

/// <summary>Content-free finding: code, field path and line only. No column can hold a field value (patient-data safety). Insert-only.</summary>
public sealed class ValidationFinding : Entity, IOrganizationOwned
{
    private ValidationFinding() { }

    public Guid OrganizationId { get; private set; }
    public Guid MessageId { get; private set; }
    public Guid RunId { get; private set; }
    public FindingSeverity Severity { get; private set; }
    public FindingCategory Category { get; private set; }
    public string Code { get; private set; } = null!;
    public string? FieldPath { get; private set; }
    public int? LineNumber { get; private set; }
    public int OccurrenceCount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static ValidationFinding Record(
        ProcessingRun run,
        FindingSeverity severity,
        FindingCategory category,
        string code,
        string? fieldPath,
        int? lineNumber,
        int occurrenceCount,
        DateTimeOffset now
    )
    {
        ArgumentNullException.ThrowIfNull(run);
        return new ValidationFinding
        {
            Id = Guid.NewGuid(),
            OrganizationId = run.OrganizationId,
            MessageId = run.MessageId,
            RunId = run.Id,
            Severity = Guard.Defined(severity, nameof(severity)),
            Category = Guard.Defined(category, nameof(category)),
            Code = Guard.Required(code, nameof(code), 60),
            FieldPath = Guard.Optional(fieldPath, nameof(fieldPath), 300),
            LineNumber = lineNumber is null
                ? null
                : Guard.InRange(lineNumber.Value, 1, int.MaxValue, nameof(lineNumber)),
            OccurrenceCount = Guard.InRange(
                occurrenceCount,
                1,
                int.MaxValue,
                nameof(occurrenceCount)
            ),
            CreatedAt = now,
        };
    }
}

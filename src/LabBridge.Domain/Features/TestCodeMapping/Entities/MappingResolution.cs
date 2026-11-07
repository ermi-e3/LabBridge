using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.TestCodeMapping.Enums;

namespace LabBridge.Domain.Features.TestCodeMapping.Entities;

/// <summary>Append-only record of how one code (line) was resolved in a run; shows source/target versions and any unit conversion applied.</summary>
public sealed class MappingResolution : Entity, IOrganizationOwned
{
    private MappingResolution() { }

    public Guid OrganizationId { get; private set; }
    public Guid RunId { get; private set; }
    public int LineNumber { get; private set; }
    public MappingDomain Domain { get; private set; }
    public string? SourceCode { get; private set; }
    public string? SourceCodeSystem { get; private set; }
    public Guid? ResolvedConceptId { get; private set; }
    public string? TargetCode { get; private set; }
    public Guid? SourceVersionId { get; private set; }
    public Guid? TargetVersionId { get; private set; }
    public ResolutionType ResolutionType { get; private set; }
    public MappingGapKind? FailureReason { get; private set; }
    public Guid? ConversionRuleId { get; private set; }
    public bool ConversionApplied { get; private set; }
    public decimal? OriginalNumeric { get; private set; }
    public decimal? ConvertedNumeric { get; private set; }

    public static MappingResolution Record(
        MappingRun run,
        int lineNumber,
        MappingDomain domain,
        string? sourceCode,
        string? sourceCodeSystem,
        Guid? resolvedConceptId,
        string? targetCode,
        Guid? sourceVersionId,
        Guid? targetVersionId,
        ResolutionType type,
        MappingGapKind? failureReason,
        UnitConversionRule? conversion,
        decimal? originalNumeric,
        decimal? convertedNumeric
    )
    {
        ArgumentNullException.ThrowIfNull(run);
        if ((type == ResolutionType.Failed) != (failureReason is not null))
            throw new ValidationException(
                "RESOLUTION_FAILURE_INVALID",
                "Invalid resolution",
                "A failure reason is required exactly when the resolution failed."
            );
        return new MappingResolution
        {
            Id = Guid.NewGuid(),
            OrganizationId = run.OrganizationId,
            RunId = run.Id,
            LineNumber = Guard.InRange(lineNumber, 0, int.MaxValue, nameof(lineNumber)),
            Domain = Guard.Defined(domain, nameof(domain)),
            SourceCode = Guard.Optional(sourceCode, nameof(sourceCode), 100),
            SourceCodeSystem = Guard.Optional(sourceCodeSystem, nameof(sourceCodeSystem), 100),
            ResolvedConceptId = resolvedConceptId,
            TargetCode = Guard.Optional(targetCode, nameof(targetCode), 100),
            SourceVersionId = sourceVersionId,
            TargetVersionId = targetVersionId,
            ResolutionType = Guard.Defined(type, nameof(type)),
            FailureReason = failureReason,
            ConversionRuleId = conversion?.Id,
            ConversionApplied = conversion is not null,
            OriginalNumeric = originalNumeric,
            ConvertedNumeric = convertedNumeric,
        };
    }
}

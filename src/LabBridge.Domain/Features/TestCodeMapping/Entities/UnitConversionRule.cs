using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.TestCodeMapping.Enums;

namespace LabBridge.Domain.Features.TestCodeMapping.Entities;

/// <summary>
/// Evidence-backed conversion of a test result between two units: converted = value * Multiplier + Offset, optionally rounded.
/// Only Approved rules are applied; at most one Approved rule exists per (test, from unit, to unit). Never edited after approval; replaced by a new rule that supersedes it.
/// </summary>
public sealed class UnitConversionRule : AggregateRoot, IOrganizationOwned
{
    private UnitConversionRule() { }

    public Guid OrganizationId { get; private set; }
    public Guid TestConceptId { get; private set; }
    public Guid FromUnitConceptId { get; private set; }
    public Guid ToUnitConceptId { get; private set; }
    public decimal Multiplier { get; private set; }
    public decimal OffsetValue { get; private set; }
    public int? RoundingDecimalPlaces { get; private set; }
    public RoundingMode? RoundingMode { get; private set; }
    public string EvidenceReference { get; private set; } = null!;
    public RuleStatus Status { get; private set; }
    public Guid? SupersedesRuleId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTimeOffset? RetiredAt { get; private set; }
    public Guid? RetiredBy { get; private set; }

    public static UnitConversionRule Draft(
        Guid organizationId,
        Guid testConceptId,
        Guid fromUnitConceptId,
        Guid toUnitConceptId,
        decimal multiplier,
        decimal offsetValue,
        int? roundingDecimalPlaces,
        RoundingMode? roundingMode,
        string evidenceReference,
        Guid? supersedesRuleId,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        if (fromUnitConceptId == toUnitConceptId)
            throw new ValidationException(
                "CONVERSION_UNITS_IDENTICAL",
                "Identical units",
                "The source and target units must differ."
            );
        if (multiplier == 0)
            throw new ValidationException(
                "CONVERSION_MULTIPLIER_INVALID",
                "Invalid multiplier",
                "The multiplier must not be zero."
            );
        if ((roundingDecimalPlaces is null) != (roundingMode is null))
            throw new ValidationException(
                "CONVERSION_ROUNDING_INVALID",
                "Invalid rounding",
                "Rounding places and mode must be provided together."
            );
        return new UnitConversionRule
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
            TestConceptId = Guard.NotEmpty(testConceptId, nameof(testConceptId)),
            FromUnitConceptId = Guard.NotEmpty(fromUnitConceptId, nameof(fromUnitConceptId)),
            ToUnitConceptId = Guard.NotEmpty(toUnitConceptId, nameof(toUnitConceptId)),
            Multiplier = multiplier,
            OffsetValue = offsetValue,
            RoundingDecimalPlaces = roundingDecimalPlaces is null
                ? null
                : Guard.InRange(roundingDecimalPlaces.Value, 0, 18, nameof(roundingDecimalPlaces)),
            RoundingMode = roundingMode is null
                ? null
                : Guard.Defined(roundingMode.Value, nameof(roundingMode)),
            EvidenceReference = Guard.Required(evidenceReference, nameof(evidenceReference), 500),
            SupersedesRuleId = supersedesRuleId,
            Status = RuleStatus.Draft,
            CreatedAt = now,
            CreatedBy = actorId,
        };
    }

    public void Approve(Guid approverId, UnitConversionRule? superseded, DateTimeOffset now)
    {
        if (Status != RuleStatus.Draft)
            throw new BusinessRuleException(
                "CONVERSION_RULE_INVALID_STATE",
                "Invalid rule state",
                "Only a draft rule can be approved."
            );

        // CHANGE: a replacement and the rule it replaces change state together, so the "one approved rule per test and units" index is never violated.
        if (SupersedesRuleId is { } supersededId)
        {
            if (superseded is null || superseded.Id != supersededId)
                throw new ValidationException(
                    "CONVERSION_SUPERSEDED_REQUIRED",
                    "Superseded rule required",
                    "The rule this one replaces must be supplied."
                );
            if (superseded.OrganizationId != OrganizationId
                || superseded.TestConceptId != TestConceptId
                || superseded.FromUnitConceptId != FromUnitConceptId
                || superseded.ToUnitConceptId != ToUnitConceptId)
                throw new ValidationException(
                    "CONVERSION_SUPERSEDED_MISMATCH",
                    "Superseded rule mismatch",
                    "The superseded rule must be for the same test and units."
                );
            if (superseded.Status == RuleStatus.Approved)
                superseded.Retire(approverId, now);
        }
        else if (superseded is not null)
        {
            throw new ValidationException(
                "CONVERSION_SUPERSEDED_UNEXPECTED",
                "Unexpected superseded rule",
                "This rule does not replace another rule."
            );
        }
        Status = RuleStatus.Approved;
        ApprovedAt = now;
        ApprovedBy = Guard.NotEmpty(approverId, nameof(approverId));
    }

    public void Retire(Guid actorId, DateTimeOffset now)
    {
        if (Status != RuleStatus.Approved)
            throw new BusinessRuleException(
                "CONVERSION_RULE_INVALID_STATE",
                "Invalid rule state",
                "Only an approved rule can be retired."
            );
        Status = RuleStatus.Retired;
        RetiredAt = now;
        RetiredBy = Guard.NotEmpty(actorId, nameof(actorId));
    }

    /// <summary>Applies the rule. Only approved rules may be used; precision is preserved in decimal arithmetic (never binary floating point).</summary>
    public decimal Convert(decimal value)
    {
        if (Status != RuleStatus.Approved)
            throw new BusinessRuleException(
                "CONVERSION_RULE_NOT_APPROVED",
                "Rule not approved",
                "Only an approved conversion rule can be applied."
            );
        //var converted = value * Multiplier + OffsetValue;
        decimal converted;
        try
        {
            converted = value * Multiplier + OffsetValue;
        }
        catch (OverflowException)
        {
            throw new BusinessRuleException(
                "CONVERSION_OVERFLOW", "Conversion overflow", "The converted value is outside the supported range.");
        }
        if (RoundingDecimalPlaces is not { } places || RoundingMode is not { } mode)
            return converted;
        return mode switch
        {
            Enums.RoundingMode.HalfUp => Math.Round(
                converted,
                places,
                MidpointRounding.AwayFromZero
            ),
            Enums.RoundingMode.HalfEven => Math.Round(
                converted,
                places,
                MidpointRounding.ToEven
            ),
            _ => Math.Round(converted, places, MidpointRounding.ToZero),
        };
    }
}

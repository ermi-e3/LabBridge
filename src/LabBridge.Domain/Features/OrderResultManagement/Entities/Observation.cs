using LabBridge.Domain.Common;

namespace LabBridge.Domain.Features.OrderResultManagement.Entities;

/// <summary>
/// One measured value of a result. The ORIGINAL value is always retained next to any converted/destination value (clinical safety).
/// Numeric values use decimal arithmetic; flags are instrument flags, never interpretation by LabBridge.
/// </summary>
public sealed class Observation : Entity, IOrganizationOwned
{
    private readonly List<string> _instrumentFlags = [];

    private Observation() { }

    internal Observation(Guid organizationId, Guid resultId, ObservationData d)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        ResultId = resultId;
        OrderLineId = d.OrderLineId;
        LineNumber = Guard.InRange(d.LineNumber, 1, int.MaxValue, nameof(d.LineNumber));
        ExternalTestCode = Guard.Required(d.ExternalTestCode, nameof(d.ExternalTestCode), 100);
        ExternalCodeSystem = Guard.Optional(
            d.ExternalCodeSystem,
            nameof(d.ExternalCodeSystem),
            100
        );
        ValueType = Guard.Required(d.ValueType, nameof(d.ValueType), 30);
        OriginalValue = Guard.RequiredExact(d.OriginalValue, nameof(d.OriginalValue), 2000);
        NumericValue = d.NumericValue;
        OriginalUnit = Guard.Optional(d.OriginalUnit, nameof(d.OriginalUnit), 100);
        InstrumentReferenceRangeText = Guard.Optional(
            d.InstrumentReferenceRangeText,
            nameof(d.InstrumentReferenceRangeText),
            200
        );
        ObservationStatus = Guard.Optional(d.ObservationStatus, nameof(d.ObservationStatus), 30);
        ObservedAt = d.ObservedAt;
        ObservedAtRaw = Guard.Optional(d.ObservedAtRaw, nameof(d.ObservedAtRaw), 100);
        if (d.InstrumentFlags is not null)
            _instrumentFlags.AddRange(d.InstrumentFlags);
    }

    public Guid OrganizationId { get; private set; }
    public Guid ResultId { get; private set; }
    public Guid? OrderLineId { get; private set; }
    public int LineNumber { get; private set; }
    public string ExternalTestCode { get; private set; } = null!;
    public string? ExternalCodeSystem { get; private set; }
    public string? CanonicalTestCode { get; private set; }
    public string? DestinationTestCode { get; private set; }
    public string ValueType { get; private set; } = null!;
    public string OriginalValue { get; private set; } = null!;
    public decimal? NumericValue { get; private set; }
    public string? OriginalUnit { get; private set; }
    public string? CanonicalUnit { get; private set; }
    public string? DestinationUnit { get; private set; }
    public string? DestinationValueText { get; private set; }
    public decimal? DestinationValueNumeric { get; private set; }
    public bool ConversionApplied { get; private set; }
    public Guid? ConversionRuleId { get; private set; }
    public IReadOnlyCollection<string> InstrumentFlags => _instrumentFlags.AsReadOnly();
    public string? InstrumentReferenceRangeText { get; private set; }
    public string? ObservationStatus { get; private set; }
    public DateTimeOffset? ObservedAt { get; private set; }
    public string? ObservedAtRaw { get; private set; }

    internal void LinkOrderLine(Guid? orderLineId) => OrderLineId = orderLineId;

    internal void ApplyMapping(
        string? canonicalTestCode,
        string? destinationTestCode,
        string? canonicalUnit,
        string? destinationUnit,
        string? destinationValueText,
        decimal? destinationValueNumeric,
        Guid? conversionRuleId
    )
    {
        CanonicalTestCode = Guard.Optional(canonicalTestCode, nameof(canonicalTestCode), 100);
        DestinationTestCode = Guard.Optional(destinationTestCode, nameof(destinationTestCode), 100);
        CanonicalUnit = Guard.Optional(canonicalUnit, nameof(canonicalUnit), 100);
        DestinationUnit = Guard.Optional(destinationUnit, nameof(destinationUnit), 100);
        DestinationValueText = Guard.Optional(
            destinationValueText,
            nameof(destinationValueText),
            2000
        );
        DestinationValueNumeric = destinationValueNumeric;
        ConversionRuleId = conversionRuleId;
        ConversionApplied = conversionRuleId is not null;
    }
}

/// <summary>Input data of an observation as normalized from the instrument message.</summary>
public sealed record ObservationData
{
    public Guid? OrderLineId { get; init; }
    public int LineNumber { get; init; }
    public string ExternalTestCode { get; init; } = null!;
    public string? ExternalCodeSystem { get; init; }
    public string ValueType { get; init; } = null!;
    public string OriginalValue { get; init; } = null!;
    public decimal? NumericValue { get; init; }
    public string? OriginalUnit { get; init; }
    public IReadOnlyCollection<string>? InstrumentFlags { get; init; }
    public string? InstrumentReferenceRangeText { get; init; }
    public string? ObservationStatus { get; init; }
    public DateTimeOffset? ObservedAt { get; init; }
    public string? ObservedAtRaw { get; init; }
}

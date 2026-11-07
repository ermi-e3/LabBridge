using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;

namespace LabBridge.Domain.Features.Reporting.Entities;

/// <summary>
/// Pre-aggregated operational fact per (organization, date, fact type, dimensions key, simulated flag); written by atomic upsert.
/// Aggregates only, no patient data; dimensions are limited to the allow-list. Simulated and real facts are never mixed (IsSimulated is part of the key).
/// </summary>
public sealed class DailyFact : Entity, IOrganizationOwned
{
    private DailyFact() { }

    public Guid OrganizationId { get; private set; }
    public DateOnly FactDate { get; private set; }
    public string FactType { get; private set; } = null!;
    public string DimensionsKey { get; private set; } = null!;
    public bool IsSimulated { get; private set; }
    public string DimensionsJson { get; private set; } = null!;
    public long FactCount { get; private set; }
    public decimal? SumValue { get; private set; }
    public decimal? MinValue { get; private set; }
    public decimal? MaxValue { get; private set; }
    public string? HistogramJson { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static DailyFact Start(
        Guid organizationId,
        DateOnly factDate,
        string factType,
        string dimensionsKey,
        bool isSimulated,
        string dimensionsJson,
        DateTimeOffset now
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
            FactDate = factDate,
            FactType = Guard.Required(factType, nameof(factType), 100),
            DimensionsKey = Guard.RequiredExact(dimensionsKey, nameof(dimensionsKey), 300),
            IsSimulated = isSimulated,
            //DimensionsJson = Guard.RequiredExact(
            //    dimensionsJson,
            //    nameof(dimensionsJson),
            //    int.MaxValue
            //),
            DimensionsJson = Guard.RequiredJson(dimensionsJson, nameof(dimensionsJson)),
            UpdatedAt = now,
        };

    /// <summary>Adds one occurrence (optionally with a measured value) to the aggregate.</summary>
    public void Add(decimal? value, DateTimeOffset now)
    {
        FactCount++;
        if (value is { } v)
        {
            SumValue = (SumValue ?? 0) + v;
            MinValue = MinValue is { } min ? Math.Min(min, v) : v;
            MaxValue = MaxValue is { } max ? Math.Max(max, v) : v;
        }
        UpdatedAt = now;
    }

    public void SetHistogram(string histogramJson, DateTimeOffset now)
    {
        //HistogramJson = histogramJson;
        HistogramJson = Guard.RequiredJson(histogramJson, nameof(histogramJson));
        UpdatedAt = now;
    }
}

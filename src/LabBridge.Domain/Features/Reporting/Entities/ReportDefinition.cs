using LabBridge.Domain.Common;

namespace LabBridge.Domain.Features.Reporting.Entities;

/// <summary>Report catalogue entry (platform reference data seeded by migration; never deleted, only deactivated). Natural key: the report code.</summary>
public sealed class ReportDefinition
{
    private ReportDefinition() { }

    public string ReportCode { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string ParameterSchemaJson { get; private set; } = null!;
    public string SourcesJson { get; private set; } = null!;
    public string DimensionsJson { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static ReportDefinition Define(
        string reportCode,
        string description,
        string parameterSchemaJson,
        string sourcesJson,
        string dimensionsJson,
        DateTimeOffset now
    ) =>
        new()
        {
            ReportCode = Guard.Required(reportCode, nameof(reportCode), 100),
            Description = Guard.Required(description, nameof(description), 500),
            //ParameterSchemaJson = Guard.RequiredExact(
            //    parameterSchemaJson,
            //    nameof(parameterSchemaJson),
            //    int.MaxValue
            //),
            //SourcesJson = Guard.RequiredExact(sourcesJson, nameof(sourcesJson), int.MaxValue),
            //DimensionsJson = Guard.RequiredExact(
            //    dimensionsJson,
            //    nameof(dimensionsJson),
            //    int.MaxValue
            //),
            ParameterSchemaJson = Guard.RequiredJson(parameterSchemaJson, nameof(parameterSchemaJson)),
            SourcesJson = Guard.RequiredJson(sourcesJson, nameof(sourcesJson)),
            DimensionsJson = Guard.RequiredJson(dimensionsJson, nameof(dimensionsJson)),
            IsActive = true,
            CreatedAt = now,
        };

    public void Deactivate() => IsActive = false;
}

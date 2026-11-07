namespace LabBridge.Domain.Features.OrderResultManagement;

/// <summary>Identifiers carried by a result message (used for correlation).</summary>
public sealed record ResultData
{
    public string? PlacerOrderId { get; init; }
    public string? FillerOrderId { get; init; }
    public string? AccessionNumber { get; init; }
    public string? SpecimenId { get; init; }
    public string? MessageControlId { get; init; }
}

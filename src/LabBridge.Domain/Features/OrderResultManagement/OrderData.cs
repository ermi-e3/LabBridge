namespace LabBridge.Domain.Features.OrderResultManagement;

/// <summary>Business data of an order as normalized from the source message. Patient fields are optional per contract and profile; all of it is protected data.</summary>
public sealed record OrderData
{
    public string? PlacerOrderId { get; init; }
    public string? FillerOrderId { get; init; }
    public string? AccessionNumber { get; init; }
    public string? SpecimenId { get; init; }
    public string? MessageControlId { get; init; }
    public string? PatientExternalId { get; init; }
    public string? PatientName { get; init; }
    public DateOnly? PatientDateOfBirth { get; init; }
    public string? PatientSex { get; init; }
    public string? SpecimenType { get; init; }
    public DateTimeOffset? SpecimenCollectedAt { get; init; }
    public string? SpecimenCollectedAtRaw { get; init; }
    public string? Priority { get; init; }
    public DateTimeOffset? OrderedAt { get; init; }
    public string? OrderedAtRaw { get; init; }
}

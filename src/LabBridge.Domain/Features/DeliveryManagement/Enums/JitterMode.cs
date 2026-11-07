namespace LabBridge.Domain.Features.DeliveryManagement.Enums;

/// <summary>Retry jitter mode. ASSUMPTION A-DLV-01: the FRS stores a jitter varchar without enumerating values.</summary>
public enum JitterMode
{
    None = 0,
    Full = 1,
    Equal = 2,
}

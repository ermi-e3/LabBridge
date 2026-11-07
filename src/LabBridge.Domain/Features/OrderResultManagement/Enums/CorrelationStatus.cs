namespace LabBridge.Domain.Features.OrderResultManagement.Enums;

/// <summary>Result-to-order correlation outcome.</summary>
public enum CorrelationStatus
{
    Unmatched = 0,
    Matched = 1,
    Ambiguous = 2,
}

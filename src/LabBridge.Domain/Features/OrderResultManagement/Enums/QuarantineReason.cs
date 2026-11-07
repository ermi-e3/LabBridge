namespace LabBridge.Domain.Features.OrderResultManagement.Enums;

/// <summary>Why an order or result is quarantined.</summary>
public enum QuarantineReason
{
    ConflictingOrderIdentifier = 0,
    NoEffectiveRoute = 1,
    PossibleCorrection = 2,
    DeliveryQuarantine = 3
}

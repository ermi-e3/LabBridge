namespace LabBridge.Domain.Features.DeliveryManagement.Enums;

/// <summary>Scripted behaviour of the outbound simulator (FR-DLV-021, six modes).</summary>
public enum SimulatorMode
{
    PositiveAck = 0,
    NegativeRetryable = 1,
    NegativePermanent = 2,
    NoResponse = 3,
    ConnectionRefused = 4,
    DelayedAck = 5,
}

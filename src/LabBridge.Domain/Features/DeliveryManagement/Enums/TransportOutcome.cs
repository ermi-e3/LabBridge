namespace LabBridge.Domain.Features.DeliveryManagement.Enums;

/// <summary>Outcome of the transport send of an attempt.</summary>
public enum TransportOutcome
{
    NotAttempted = 0,
    Sent = 1,
    ConnectionFailed = 2,
    SendFailed = 3,
    TimedOut = 4,
    Unknown = 5,
}

namespace LabBridge.Domain.Features.MessageIngestion.Enums;

/// <summary>Why a message that passed source resolution was refused (BR-ING-002, 009, 015, 016).</summary>
public enum RefusalReason
{
    OrganizationPolicy = 0,
    ConnectorNotEnabled = 1,
    PayloadTooLarge = 2,
    EmptyPayload = 3,
    CapacityExceeded = 4,
}

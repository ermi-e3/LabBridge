namespace LabBridge.Domain.Features.MessageIngestion.Enums;

/// <summary>Outcome of receipt at the connector boundary.</summary>
public enum ReceiptOutcome
{
    Quarantined = 0,
    Accepted = 1,
    Duplicate = 2,
}

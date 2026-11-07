namespace LabBridge.Domain.Common.Enums;

/// <summary>Kind of message (Stage 1 section 10.2). Query is a later-phase kind.</summary>
public enum MessageKind
{
    Order = 0,
    Result = 1,
    Acknowledgment = 2,
    Query = 3,
    Other = 4
}

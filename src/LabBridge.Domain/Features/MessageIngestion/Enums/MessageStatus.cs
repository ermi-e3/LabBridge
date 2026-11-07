namespace LabBridge.Domain.Features.MessageIngestion.Enums;

/// <summary>Message pipeline status (Stage 1 section 8.2). Ingestion owns Received/Duplicate/Quarantined; Module 5 owns the processing states.</summary>
public enum MessageStatus
{
    Quarantined = 0,
    Received = 1,
    Duplicate = 2,
    Parsing = 3,
    Unsupported = 4,
    Invalid = 5,
    Normalized = 6,
    Processed = 7,
    ProcessingFailed = 8,
}

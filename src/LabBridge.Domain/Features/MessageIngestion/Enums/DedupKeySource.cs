namespace LabBridge.Domain.Features.MessageIngestion.Enums;

/// <summary>Whether the deduplication key came from profile-defined identifiers or the content hash (BR-ING-005).</summary>
public enum DedupKeySource
{
    Identifier = 0,
    ContentHash = 1,
}

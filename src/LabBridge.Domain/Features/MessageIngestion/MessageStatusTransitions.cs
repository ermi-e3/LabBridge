using LabBridge.Domain.Features.MessageIngestion.Enums;

namespace LabBridge.Domain.Features.MessageIngestion;

/// <summary>
/// Allowed message status transitions. Ingestion transitions come from Module 4 section 4.0; processing transitions follow Stage 1 section 8.2
/// (ASSUMPTION A-ING-01: Module 5 confirms the processing part; any non-listed transition is invalid).
/// </summary>
public static class MessageStatusTransitions
{
    private static readonly Dictionary<MessageStatus, MessageStatus[]> Allowed = new()
    {
        [MessageStatus.Received] = [MessageStatus.Parsing, MessageStatus.Quarantined],
        [MessageStatus.Parsing] =
        [
            MessageStatus.Normalized,
            MessageStatus.Unsupported,
            MessageStatus.Invalid,
            MessageStatus.ProcessingFailed,
            MessageStatus.Quarantined,
        ],
        [MessageStatus.Normalized] =
        [
            MessageStatus.Processed,
            MessageStatus.ProcessingFailed,
            MessageStatus.Quarantined,
        ],
        [MessageStatus.ProcessingFailed] = [MessageStatus.Parsing, MessageStatus.Quarantined],
        [MessageStatus.Unsupported] = [MessageStatus.Quarantined],
        [MessageStatus.Invalid] = [MessageStatus.Quarantined],
        [MessageStatus.Quarantined] = [MessageStatus.Received, MessageStatus.Duplicate],
        [MessageStatus.Duplicate] = [],
        [MessageStatus.Processed] = [],
    };

    public static bool IsAllowed(MessageStatus from, MessageStatus to) =>
        Allowed.TryGetValue(from, out var targets) && targets.Contains(to);
}

using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Enums;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Common.ValueObjects;
using LabBridge.Domain.Features.MessageProcessing.Enums;
using System.Text.Json;

namespace LabBridge.Domain.Features.MessageProcessing.Entities;

/// <summary>
/// Canonical-schema representation of a message, versioned per message; exactly one version is current.
/// ContentJson is protected clinical data and is cleared (never the row) on purge. Acknowledgments go to the Delivery consumer.
/// ASSUMPTION A-PRC-01: consumer = Delivery for Acknowledgment, Workflow for every other kind (the FRS fixes only the Acknowledgment rule).
/// </summary>
public sealed class NormalizedMessage : AggregateRoot, IOrganizationOwned
{
    private NormalizedMessage() { }

    public Guid OrganizationId { get; private set; }
    public Guid MessageId { get; private set; }
    public Guid RunId { get; private set; }
    public int Version { get; private set; }
    public bool IsCurrent { get; private set; }
    public MessageKind MessageKind { get; private set; }
    public int CanonicalSchemaVersion { get; private set; }
    public string? ContentJson { get; private set; }
    public string ContentHash { get; private set; } = null!;
    public ConsumerKind Consumer { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? PurgedAt { get; private set; }

    public static ConsumerKind ConsumerFor(MessageKind kind) =>
        kind == MessageKind.Acknowledgment ? ConsumerKind.Delivery : ConsumerKind.Workflow;

    public static NormalizedMessage Create(
     ProcessingRun run,
     NormalizedMessage? previousCurrent,
     MessageKind kind,
     int canonicalSchemaVersion,
     string contentJson,
     Sha256Hash contentHash,
     DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(run);
        ArgumentNullException.ThrowIfNull(contentHash);
        if (
            run.Status != ProcessingRunStatus.Completed
            || run.Outcome != ProcessingOutcome.Normalized
        )
            throw new BusinessRuleException(
                "NORMALIZED_RUN_INVALID",
                "Run not normalized",
                "A normalized message can only be created from a run that completed as Normalized."
            );

        if (run.MessageKind is { } runKind && runKind != kind)
            throw new ValidationException("NORMALIZED_KIND_MISMATCH", "Message kind mismatch",
                "The normalized message kind must match the processing run.");
        if (previousCurrent is not null && previousCurrent.MessageId != run.MessageId)
            throw new ValidationException("NORMALIZED_MESSAGE_MISMATCH", "Message mismatch",
                "The previous version belongs to a different message.");
        try { using var _ = JsonDocument.Parse(contentJson); }
        catch (JsonException)
        {
            throw new ValidationException("NORMALIZED_CONTENT_INVALID", "Invalid content", "The normalized content must be valid JSON.");
        }

        previousCurrent?.Supersede();

        return new NormalizedMessage
        {
            Id = Guid.NewGuid(),
            OrganizationId = run.OrganizationId,
            MessageId = run.MessageId,
            RunId = run.Id,
            Version = (previousCurrent?.Version ?? 0) + 1,
            IsCurrent = true,
            MessageKind = Guard.Defined(kind, nameof(kind)),
            CanonicalSchemaVersion = Guard.InRange(
                canonicalSchemaVersion,
                1,
                int.MaxValue,
                nameof(canonicalSchemaVersion)
            ),
            ContentJson = Guard.RequiredExact(contentJson, nameof(contentJson), int.MaxValue),
            ContentHash = contentHash.Value,
            Consumer = ConsumerFor(kind),
            CreatedAt = now,
        };
    }

    /// <summary>Called when a newer version becomes current (exactly one current version per message).</summary>
    public void Supersede() => IsCurrent = false;

    public void Purge(DateTimeOffset now)
    {
        if (PurgedAt is not null)
            return;
        ContentJson = null;
        PurgedAt = now;
    }
}

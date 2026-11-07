using LabBridge.Domain.Common;
using LabBridge.Domain.Features.InstrumentManagement.Enums;

namespace LabBridge.Domain.Features.InstrumentManagement.Entities;

/// <summary>Append-only verification evidence for an instrument against a profile version. No update or delete path.</summary>
public sealed class InstrumentVerification : Entity, IOrganizationOwned
{
    private InstrumentVerification() { }

    internal InstrumentVerification(
        Guid organizationId,
        Guid instrumentId,
        Guid profileVersionId,
        VerificationKind kind,
        VerificationOutcome outcome,
        string evidenceReference,
        string? notes,
        DateTimeOffset performedAt,
        DateTimeOffset recordedAt,
        Guid? recordedBy
    )
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        InstrumentId = instrumentId;
        ProfileVersionId = Guard.NotEmpty(profileVersionId, nameof(profileVersionId));
        Kind = Guard.Defined(kind, nameof(kind));
        Outcome = Guard.Defined(outcome, nameof(outcome));
        EvidenceReference = Guard.Required(evidenceReference, nameof(evidenceReference), 500);
        Notes = Guard.Optional(notes, nameof(notes), 1000);
        PerformedAt = performedAt;
        RecordedAt = recordedAt;
        RecordedBy = recordedBy;
    }

    public Guid OrganizationId { get; private set; }
    public Guid InstrumentId { get; private set; }
    public Guid ProfileVersionId { get; private set; }
    public VerificationKind Kind { get; private set; }
    public VerificationOutcome Outcome { get; private set; }
    public string EvidenceReference { get; private set; } = null!;
    public string? Notes { get; private set; }
    public DateTimeOffset PerformedAt { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }
    public Guid? RecordedBy { get; private set; }
}

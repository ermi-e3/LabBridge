using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.InstrumentManagement.Enums;

namespace LabBridge.Domain.Features.InstrumentManagement.Entities;

/// <summary>
/// A laboratory instrument registered by an organization. Aggregate root of its verification records.
/// IsSimulated is immutable (BR-CON-007). Compatibility class, verification status and IsSimulated are always exposed together (BR-CON-013).
/// Instruments are never deleted (BR-CON-014).
/// </summary>
public sealed class Instrument : AuditedAggregateRoot, IOrganizationOwned
{
    private readonly List<InstrumentVerification> _verifications = [];

    private Instrument() { }

    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Manufacturer { get; private set; } = null!;
    public string Model { get; private set; } = null!;
    public string? SerialNumber { get; private set; }
    public string? Location { get; private set; }
    public string? Notes { get; private set; }
    public bool IsSimulated { get; private set; }
    public CompatibilityClass? CompatibilityClass { get; private set; }
    public string? CompatibilityRationale { get; private set; }
    public Guid? CompatibilityBasisProfileVersionId { get; private set; }
    public VerificationStatus VerificationStatus { get; private set; }

    /// <summary>System that sends orders for this instrument (reference to an ExternalSystem of the same organization).</summary>
    public Guid? OrderSourceSystemId { get; private set; }

    /// <summary>System that receives results of this instrument (reference to an ExternalSystem of the same organization).</summary>
    public Guid? ResultDestinationSystemId { get; private set; }
    public InstrumentTopology? Topology { get; private set; }
    public ActiveStatus Status { get; private set; }

    public IReadOnlyCollection<InstrumentVerification> Verifications => _verifications.AsReadOnly();

    public static Instrument Register(
        Guid organizationId,
        string name,
        string manufacturer,
        string model,
        string? serialNumber,
        string? location,
        string? notes,
        bool isSimulated,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        var instrument = new Instrument
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
            Name = Guard.Required(name, nameof(name), 200),
            Manufacturer = Guard.Required(manufacturer, nameof(manufacturer), 100),
            Model = Guard.Required(model, nameof(model), 100),
            SerialNumber = Guard.Optional(serialNumber, nameof(serialNumber), 100),
            Location = Guard.Optional(location, nameof(location), 200),
            Notes = Guard.Optional(notes, nameof(notes), 1000),
            IsSimulated = isSimulated,
            VerificationStatus = VerificationStatus.Unverified,
            Status = ActiveStatus.Active,
        };
        instrument.StampCreated(now, actorId);
        return instrument;
    }

    public void UpdateDetails(
        string name,
        string manufacturer,
        string model,
        string? serialNumber,
        string? location,
        string? notes,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        Name = Guard.Required(name, nameof(name), 200);
        Manufacturer = Guard.Required(manufacturer, nameof(manufacturer), 100);
        Model = Guard.Required(model, nameof(model), 100);
        SerialNumber = Guard.Optional(serialNumber, nameof(serialNumber), 100);
        Location = Guard.Optional(location, nameof(location), 200);
        Notes = Guard.Optional(notes, nameof(notes), 1000);
        StampUpdated(now, actorId);
    }

    /// <summary>
    /// Records the compatibility assessment (BR-CON-004). C1 requires a Published basis profile version; C2/C3 require a rationale.
    /// Simulated instruments have no class.
    /// </summary>
    /// <param name="hasNonSimulatedConnectors">True when the instrument already has non-simulated connectors (the class may not be lowered then).</param>
    public void AssessCompatibility(
        CompatibilityClass assessedClass,
        string? rationale,
        Guid? basisProfileVersionId,
        bool basisProfileVersionIsPublished,
        bool hasNonSimulatedConnectors,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        Guard.Defined(assessedClass, nameof(assessedClass));
        if (IsSimulated)
            throw new BusinessRuleException(
                "INSTRUMENT_SIMULATED_NO_CLASS",
                "Simulated instrument",
                "A simulated instrument has no compatibility class."
            );
        if (assessedClass == Enums.CompatibilityClass.C1)
        {
            if (basisProfileVersionId is null || basisProfileVersionId == Guid.Empty)
                throw new ValidationException(
                    "INSTRUMENT_CLASS_BASIS_REQUIRED",
                    "Basis profile version required",
                    "Class C1 requires a basis profile version."
                );
            if (!basisProfileVersionIsPublished)
                throw new ValidationException(
                    "INSTRUMENT_CLASS_BASIS_INVALID",
                    "Invalid basis profile version",
                    "The basis profile version must be Published."
                );
        }
        else if (string.IsNullOrWhiteSpace(rationale))
        {
            throw new ValidationException(
                "INSTRUMENT_CLASS_RATIONALE_REQUIRED",
                "Rationale required",
                "Classes C2 and C3 require a rationale."
            );
        }

        if (
            hasNonSimulatedConnectors
            && assessedClass != Enums.CompatibilityClass.C1
        )
            throw new ConflictException(
                "INSTRUMENT_CLASS_IN_USE",
                "Compatibility class in use",
                "Non-simulated connectors depend on the current class."
            );

        CompatibilityClass = assessedClass;
        CompatibilityRationale = Guard.Optional(rationale, nameof(rationale), 1000);
        CompatibilityBasisProfileVersionId =
            assessedClass == Enums.CompatibilityClass.C1
                ? basisProfileVersionId
                : null;
        StampUpdated(now, actorId);
    }

    /// <summary>
    /// Sets order-source / result-destination systems and the declared topology. Declared topology must equal the derived one (BR-CON-021);
    /// changes are blocked while an Enabled route depends on them (BR-CON-010).
    /// </summary>
    public void ConfigureSystems(
        Guid? orderSourceSystemId,
        Guid? resultDestinationSystemId,
        InstrumentTopology? declaredTopology,
        bool dependedOnByEnabledRoute,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        var changed =
            orderSourceSystemId != OrderSourceSystemId
            || resultDestinationSystemId != ResultDestinationSystemId;
        if (changed && dependedOnByEnabledRoute)
            throw new ConflictException(
                "INSTRUMENT_IN_USE",
                "Instrument in use",
                "An enabled route depends on the current systems."
            );

        InstrumentTopology? derived =
            orderSourceSystemId is { } o && resultDestinationSystemId is { } r
                ? (o == r ? InstrumentTopology.SameSystem : InstrumentTopology.DifferentSystems)
                : null;
        if (declaredTopology is not null && declaredTopology != derived)
            throw new ValidationException(
                "INSTRUMENT_TOPOLOGY_MISMATCH",
                "Topology mismatch",
                "The declared topology does not match the configured systems."
            );

        OrderSourceSystemId = Guard.NotEmptyWhenSet(
            orderSourceSystemId,
            nameof(orderSourceSystemId)
        );
        ResultDestinationSystemId = Guard.NotEmptyWhenSet(
            resultDestinationSystemId,
            nameof(resultDestinationSystemId)
        );
        Topology = derived;
        StampUpdated(now, actorId);
    }

    /// <summary>
    /// Adds a verification record and recomputes the verification status in the same operation (BR-CON-005).
    /// DECISION D-CON-01: a level counts only when the LATEST record of that kind for the profile version has outcome Passed.
    /// </summary>
    public InstrumentVerification RecordVerification(
        Guid profileVersionId,
        VerificationKind kind,
        VerificationOutcome outcome,
        string evidenceReference,
        string? notes,
        DateTimeOffset performedAt,
        bool hasNonSimulatedConnector,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        if (IsSimulated && kind == VerificationKind.Hardware)
            throw new BusinessRuleException(
                "INSTRUMENT_SIMULATED_NO_HARDWARE_VERIFICATION",
                "Invalid verification",
                "A simulated instrument cannot receive hardware verification."
            );
        var verification = new InstrumentVerification(
            OrganizationId,
            Id,
            profileVersionId,
            kind,
            outcome,
            evidenceReference,
            notes,
            performedAt,
            now,
            actorId
        );
        _verifications.Add(verification);
        VerificationStatus = DeriveVerificationStatus(profileVersionId, hasNonSimulatedConnector);
        StampUpdated(now, actorId);
        return verification;
    }

    /// <summary>Derives the status for a profile version from the stored verification records.</summary>
    public VerificationStatus DeriveVerificationStatus(
        Guid currentProfileVersionId,
        bool hasNonSimulatedConnector
    )
    {
        InstrumentVerification? Latest(VerificationKind kind) =>
            _verifications
                .Where(v => v.ProfileVersionId == currentProfileVersionId && v.Kind == kind)
                .OrderByDescending(v => v.PerformedAt)
                .ThenByDescending(v => v.RecordedAt)
                .FirstOrDefault();

        if (
            !IsSimulated
            && hasNonSimulatedConnector
            && Latest(VerificationKind.Hardware) is { Outcome: VerificationOutcome.Passed }
        )
            return VerificationStatus.HardwareVerified;
        if (Latest(VerificationKind.Simulator) is { Outcome: VerificationOutcome.Passed })
            return VerificationStatus.SimulatorVerified;
        return VerificationStatus.Unverified;
    }

    /// <param name="inUse">True while Enabled connectors or routes depend on the instrument (BR-CON-014).</param>
    public void Deactivate(bool inUse, DateTimeOffset now, Guid? actorId)
    {
        if (Status == ActiveStatus.Inactive)
            return;
        if (inUse)
            throw new ConflictException(
                "INSTRUMENT_IN_USE",
                "Instrument in use",
                "The instrument is used by enabled connectors or routes."
            );
        Status = ActiveStatus.Inactive;
        StampUpdated(now, actorId);
    }

    public void Activate(DateTimeOffset now, Guid? actorId)
    {
        if (Status == ActiveStatus.Active)
            return;
        Status = ActiveStatus.Active;
        StampUpdated(now, actorId);
    }

    public void EnsureActive()
    {
        if (Status != ActiveStatus.Active)
            throw new BusinessRuleException(
                "INSTRUMENT_INACTIVE",
                "Instrument inactive",
                "The operation requires an active instrument."
            );
    }
}

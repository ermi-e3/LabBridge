using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.OrganizationManagement.Enums;

namespace LabBridge.Domain.Features.OrganizationManagement.Entities;

/// <summary>
/// The tenant root (Module 2). Aggregate root of the organization's status history. Organizations are never deleted in the MVP
/// (BR-ORG-005, BR-ORG-018). Referenced by every tenant-owned entity through OrganizationId (SC-ORG-003).
/// </summary>
public sealed class Organization : AuditedAggregateRoot
{
    private readonly List<OrganizationStatusHistory> _statusHistory = [];

    private Organization() { }

    public string Code { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public OrganizationStatus Status { get; private set; }
    public SuspensionMode? SuspensionMode { get; private set; }
    public string? StatusReason { get; private set; }
    public DateTimeOffset StatusChangedAt { get; private set; }
    public Guid? StatusChangedBy { get; private set; }

    /// <summary>Monotonic version used for stale-update protection of status changes.</summary>
    public int StatusVersion { get; private set; }

    /// <summary>IANA time-zone identifier. Presentation-only; all stored times are UTC (BR-ORG-012).</summary>
    public string DefaultTimeZone { get; private set; } = null!;
    public bool InitialAdminProvisioned { get; private set; }

    public IReadOnlyCollection<OrganizationStatusHistory> StatusHistory =>
        _statusHistory.AsReadOnly();
    public OrganizationOperatingPolicy OperatingPolicy =>
        OrganizationOperatingPolicy.For(Status, SuspensionMode);

    public static Organization Provision(
        OrganizationCode code,
        string displayName,
        string? defaultTimeZone,
        DateTimeOffset now,
        Guid? actorId,
        Guid? correlationId = null
    )
    {
        ArgumentNullException.ThrowIfNull(code);
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Code = code.Value,
            DisplayName = Guard.Required(displayName, nameof(displayName), 200),
            DefaultTimeZone = NormalizeTimeZone(defaultTimeZone),
            Status = OrganizationStatus.Provisioning,
            StatusVersion = 1,
            StatusChangedAt = now,
            StatusChangedBy = actorId,
        };
        org.StampCreated(now, actorId);
        org._statusHistory.Add(
            new OrganizationStatusHistory(
                org.Id,
                null,
                OrganizationStatus.Provisioning,
                null,
                null,
                1,
                now,
                actorId,
                correlationId
            )
        );
        org.Raise(
            new OrganizationStatusChanged(
                org.Id,
                null,
                OrganizationStatus.Provisioning,
                null,
                1,
                now
            )
        );
        return org;
    }

    public void UpdateDetails(
        string? displayName,
        string? defaultTimeZone,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        Require(Status != OrganizationStatus.Deactivated, "A deactivated organization cannot be modified.");
        if (displayName is not null)
            DisplayName = Guard.Required(displayName, nameof(displayName), 200);
        if (defaultTimeZone is not null)
            DefaultTimeZone = NormalizeTimeZone(defaultTimeZone);
        StampUpdated(now, actorId);
    }

    public void MarkInitialAdministratorProvisioned(DateTimeOffset now, Guid? actorId)
    {
        Require(Status != OrganizationStatus.Deactivated, "A deactivated organization cannot be modified.");
        InitialAdminProvisioned = true;
        StampUpdated(now, actorId);
    }

    /// <summary>Provisioning -> Active. Returns false when already Active (idempotent no-op).</summary>
    public bool Activate(DateTimeOffset now, Guid? actorId, Guid? correlationId = null)
    {
        if (Status == OrganizationStatus.Active)
            return false;
        Require(
            Status == OrganizationStatus.Provisioning,
            "An organization can only be activated while it is being provisioned."
        );
        if (!InitialAdminProvisioned)
            throw new BusinessRuleException(
                "ORGANIZATION_ACTIVATION_PRECONDITION",
                "Activation precondition not met",
                "The initial administrator must be provisioned before activation."
            );
        Transition(OrganizationStatus.Active, null, null, now, actorId, correlationId);
        return true;
    }

    /// <summary>Active -> Suspended. Returns false when already suspended in the same mode; a different mode is a conflict.</summary>
    public bool Suspend(
        SuspensionMode mode,
        string reason,
        DateTimeOffset now,
        Guid? actorId,
        Guid? correlationId = null
    )
    {
        var trimmed = Guard.Required(reason, nameof(reason), 500);
        Guard.Defined(mode, nameof(mode));
        if (Status == OrganizationStatus.Suspended)
        {
            if (SuspensionMode == mode)
                return false;
            throw new ConflictException(
                "ORGANIZATION_SUSPENSION_MODE_CONFLICT",
                "Suspension mode conflict",
                "The organization is already suspended in a different mode."
            );
        }
        Require(
            Status == OrganizationStatus.Active,
            "Only an active organization can be suspended."
        );
        Transition(OrganizationStatus.Suspended, mode, trimmed, now, actorId, correlationId);
        return true;
    }

    /// <summary>Suspended -> Active. Returns false when already Active.</summary>
    public bool Reactivate(
        string? reason,
        DateTimeOffset now,
        Guid? actorId,
        Guid? correlationId = null
    )
    {
        if (Status == OrganizationStatus.Active)
            return false;
        Require(
            Status == OrganizationStatus.Suspended,
            "Only a suspended organization can be reactivated."
        );
        Transition(
            OrganizationStatus.Active,
            null,
            Guard.Optional(reason, nameof(reason), 500),
            now,
            actorId,
            correlationId
        );
        return true;
    }

    /// <summary>Provisioning/Active/Suspended -> Deactivated (terminal in the MVP). Requires the organization code as confirmation (VR-ORG-009).</summary>
    public bool Deactivate(
        string reason,
        string confirmOrganizationCode,
        DateTimeOffset now,
        Guid? actorId,
        Guid? correlationId = null
    )
    {
        var trimmed = Guard.Required(reason, nameof(reason), 500);
        if (Status == OrganizationStatus.Deactivated)
            return false;
        if (!string.Equals(Code, confirmOrganizationCode?.Trim(), StringComparison.Ordinal))
            throw new ValidationException(
                "ORGANIZATION_CONFIRMATION_MISMATCH",
                "Confirmation mismatch",
                "The confirmation does not match the organization code."
            );
        Transition(OrganizationStatus.Deactivated, null, trimmed, now, actorId, correlationId);
        return true;
    }

    private void Transition(
        OrganizationStatus to,
        SuspensionMode? mode,
        string? reason,
        DateTimeOffset now,
        Guid? actorId,
        Guid? correlationId
    )
    {
        var from = Status;
        Status = to;
        SuspensionMode = mode;
        StatusReason = reason;
        StatusChangedAt = now;
        StatusChangedBy = actorId;
        StatusVersion++;
        StampUpdated(now, actorId);
        _statusHistory.Add(
            new OrganizationStatusHistory(
                Id,
                from,
                to,
                mode,
                reason,
                StatusVersion,
                now,
                actorId,
                correlationId
            )
        );
        Raise(new OrganizationStatusChanged(Id, from, to, mode, StatusVersion, now));
    }

    private static void Require(bool condition, string detail)
    {
        if (!condition)
            throw new BusinessRuleException(
                "ORGANIZATION_INVALID_STATE",
                "Invalid organization state",
                detail
            );
    }

    private static string NormalizeTimeZone(string? value)
    {
        var id = string.IsNullOrWhiteSpace(value) ? "UTC" : value.Trim();
        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            throw new ValidationException(
                "ORGANIZATION_TIMEZONE_INVALID",
                "Invalid time zone",
                "The time zone is not a valid IANA identifier."
            );
        }
        return id;
    }
}

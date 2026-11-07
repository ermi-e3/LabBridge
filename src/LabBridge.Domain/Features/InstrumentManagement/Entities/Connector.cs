using System.Text.Json;
using System.Text.Json.Nodes;
using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.InstrumentManagement.Enums;

namespace LabBridge.Domain.Features.InstrumentManagement.Entities;

/// <summary>
/// A configured communication endpoint binding an adapter type and interface profile version to exactly one counterparty
/// (instrument XOR external system). Aggregate root of its secret references. Test results and configuration revisions are
/// insert-only records produced by this root (they can grow without bound and are therefore not held as loaded collections).
/// Direction, adapter type, counterparty and IsSimulated are immutable after creation (BR-CON-007).
/// </summary>
public sealed class Connector : AuditedAggregateRoot, IOrganizationOwned
{
    private readonly List<ConnectorSecretRef> _secretRefs = [];

    private Connector() { }

    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public ConnectorDirection Direction { get; private set; }
    public Guid? InstrumentId { get; private set; }
    public Guid? ExternalSystemId { get; private set; }
    public Guid AdapterTypeId { get; private set; }
    public Guid ProfileVersionId { get; private set; }
    public bool IsSimulated { get; private set; }

    /// <summary>Non-secret settings only (BR-CON-011).</summary>
    public string SettingsJson { get; private set; } = null!;
    public int ConfigVersion { get; private set; }
    public ConnectorStatus Status { get; private set; }

    /// <summary>Hash of the inbound binding key; platform-wide unique; required for Inbound/Bidirectional (BR-CON-018).</summary>
    public byte[]? BindingKeyHash { get; private set; }
    public bool EverEnabled { get; private set; }
    public DateTimeOffset? EnabledAt { get; private set; }
    public DateTimeOffset? DisabledAt { get; private set; }

    public IReadOnlyCollection<ConnectorSecretRef> SecretRefs => _secretRefs.AsReadOnly();
    public bool CanReceive =>
        Direction is ConnectorDirection.Inbound or ConnectorDirection.Bidirectional;
    public bool CanSend =>
        Direction is ConnectorDirection.Outbound or ConnectorDirection.Bidirectional;

    /// <summary>Creates a Draft connector (ConfigVersion 1) and its first configuration revision.</summary>
    public static (Connector Connector, ConnectorConfigRevision Revision) Create(
        Guid organizationId,
        string name,
        string? description,
        ConnectorDirection direction,
        Instrument? instrument,
        ExternalSystem? externalSystem,
        AdapterType adapterType,
        InterfaceProfileVersion profileVersion,
        string settingsJson,
        byte[]? bindingKeyHash,
        bool simulatorEnabled,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        ArgumentNullException.ThrowIfNull(adapterType);
        ArgumentNullException.ThrowIfNull(profileVersion);
        Guard.Defined(direction, nameof(direction));
        if ((instrument is null) == (externalSystem is null))
            throw new ValidationException(
                "CONNECTOR_COUNTERPARTY_INVALID",
                "Invalid counterparty",
                "A connector must reference exactly one instrument or one external system."
            );
        if (
            instrument is not null && instrument.OrganizationId != organizationId
            || externalSystem is not null && externalSystem.OrganizationId != organizationId
        )
            throw new ForbiddenException(
                "TENANT_ACCESS_DENIED",
                "Tenant access denied",
                "The counterparty does not belong to the organization."
            );
        if (!adapterType.IsActive)
            throw new BusinessRuleException(
                "CONNECTOR_ADAPTER_INACTIVE",
                "Adapter inactive",
                "The adapter type is not active."
            );
        if (!adapterType.Supports(direction))
            throw new ValidationException(
                "CONNECTOR_DIRECTION_UNSUPPORTED",
                "Direction not supported",
                "The adapter type does not support the requested direction."
            );
        if (!profileVersion.IsSelectableForNewConnector)
            throw new BusinessRuleException(
                "PROFILE_VERSION_NOT_SELECTABLE",
                "Profile version not selectable",
                "Only Published profile versions can be selected for new connectors."
            );
        if (adapterType.IsSimulator && !simulatorEnabled)
            throw new BusinessRuleException(
                "SIMULATOR_DISABLED",
                "Simulator disabled",
                "Simulators are disabled for this organization."
            );
        if (
            direction != ConnectorDirection.Outbound
            && (bindingKeyHash is null || bindingKeyHash.Length == 0)
        )
            throw new ValidationException(
                "CONNECTOR_BINDING_REQUIRED",
                "Binding key required",
                "Inbound and bidirectional connectors require an inbound binding key."
            );

        var isSimulated = adapterType.IsSimulator;
        if (instrument is not null)
        {
            instrument.EnsureActive();
            EnsureCompatibleWithInstrument(instrument, isSimulated);
        }
        else if (externalSystem!.Status != ActiveStatus.Active)
        {
            throw new BusinessRuleException(
                "EXTERNAL_SYSTEM_INACTIVE",
                "External system inactive",
                "The operation requires an active external system."
            );
        }

        var connector = new Connector
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = Guard.Required(name, nameof(name), 200),
            Description = Guard.Optional(description, nameof(description), 1000),
            Direction = direction,
            InstrumentId = instrument?.Id,
            ExternalSystemId = externalSystem?.Id,
            AdapterTypeId = adapterType.Id,
            ProfileVersionId = profileVersion.Id,
            IsSimulated = isSimulated,
            SettingsJson = RequireJson(settingsJson),
            ConfigVersion = 1,
            Status = ConnectorStatus.Draft,
            BindingKeyHash = direction == ConnectorDirection.Outbound ? null : bindingKeyHash,
        };
        connector.StampCreated(now, actorId);
        return (connector, connector.CreateRevision(now, actorId, "Created"));
    }

    /// <summary>Changes non-secret settings; allowed while Enabled (BR-CON-008). Creates a new configuration version.</summary>
    public ConnectorConfigRevision UpdateSettings(
        string settingsJson,
        string? reason,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        var json = RequireJson(settingsJson);
        if (JsonNode.DeepEquals(JsonNode.Parse(json), JsonNode.Parse(SettingsJson)))   // semantic, not textual (jsonb rewrites text)
            throw new ValidationException("CONNECTOR_NO_CHANGE", "No change", "The settings are unchanged.");
        SettingsJson = json;
        return NewVersion(now, actorId, reason ?? "Settings updated");
    }

    public void Rename(string name, string? description, DateTimeOffset now, Guid? actorId)
    {
        Name = Guard.Required(name, nameof(name), 200);
        Description = Guard.Optional(description, nameof(description), 1000);
        StampUpdated(now, actorId);
    }

    /// <summary>Stores/replaces the reference of a secret slot; a new configuration version is created (BR-CON-016).</summary>
    public ConnectorConfigRevision SetSecretReference(
        string slot,
        string secretReference,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        var existing = _secretRefs.FirstOrDefault(s => s.Slot == slot);
        if (existing is null)
            _secretRefs.Add(
                new ConnectorSecretRef(OrganizationId, Id, slot, secretReference, now, actorId)
            );
        else
            existing.Replace(secretReference, now, actorId);
        return NewVersion(now, actorId, $"Secret slot '{slot}' set");
    }

    /// <summary>Profile version may change only when not Enabled (BR-CON-008); deprecated/withdrawn versions cannot be selected.</summary>
    public ConnectorConfigRevision ChangeProfileVersion(
        InterfaceProfileVersion profileVersion,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        ArgumentNullException.ThrowIfNull(profileVersion);
        EnsureNotEnabled();
        if (!profileVersion.IsSelectableForNewConnector)
            throw new BusinessRuleException(
                "PROFILE_VERSION_NOT_SELECTABLE",
                "Profile version not selectable",
                "Only Published profile versions can be selected."
            );
        ProfileVersionId = profileVersion.Id;
        return NewVersion(now, actorId, "Profile version changed");
    }

    public ConnectorConfigRevision ChangeBindingKey(
        byte[] bindingKeyHash,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        EnsureNotEnabled();
        if (!CanReceive)
            throw new BusinessRuleException(
                "CONNECTOR_BINDING_NOT_APPLICABLE",
                "Binding not applicable",
                "Outbound connectors have no inbound binding key."
            );
        BindingKeyHash = Guard.RequiredBytes(bindingKeyHash, nameof(bindingKeyHash));
        return NewVersion(now, actorId, "Binding key changed");
    }

    public ConnectorTestResult RecordTestResult(
        ConnectorTestOutcome outcome,
        string? summary,
        string? errorCode,
        DateTimeOffset startedAt,
        DateTimeOffset completedAt,
        Guid? actorId
    ) =>
        new(
            OrganizationId,
            Id,
            ConfigVersion,
            outcome,
            summary,
            errorCode,
            startedAt,
            completedAt,
            actorId
        );

    /// <summary>
    /// Draft/Disabled -> Enabled. Verifies: counterparty active, Published (not Withdrawn) profile version, compatibility gating (BR-CON-004),
    /// simulator gating (BR-CON-006), and a Passed test for the current configuration where the adapter supports testing (BR-CON-019, Proposed).
    /// </summary>
    public void Enable(
    Instrument? instrument, ExternalSystem? externalSystem,
    InterfaceProfileVersion profileVersion, AdapterType adapterType,
    ConnectorTestResult? latestTestForCurrentConfiguration,
    bool simulatorEnabled, DateTimeOffset now, Guid? actorId)
        {
        ArgumentNullException.ThrowIfNull(profileVersion);
        ArgumentNullException.ThrowIfNull(adapterType);
        if (Status == ConnectorStatus.Enabled)
            return;
        if (
            profileVersion.Id != ProfileVersionId
            || adapterType.Id != AdapterTypeId
            || instrument?.Id != InstrumentId
            || externalSystem?.Id != ExternalSystemId
        )
            throw new ValidationException(
                "CONNECTOR_ENABLE_CONTEXT_MISMATCH",
                "Context mismatch",
                "The supplied collaborators do not match the connector configuration."
            );
        if (profileVersion.Status == ProfileVersionStatus.Withdrawn)
            throw new BusinessRuleException(
                "PROFILE_VERSION_WITHDRAWN",
                "Profile version withdrawn",
                "A connector using a withdrawn profile version cannot be enabled."
            );
        if (!adapterType.IsActive)
            throw new BusinessRuleException(
                "CONNECTOR_ADAPTER_INACTIVE",
                "Adapter inactive",
                "The adapter type is not active."
            );
        if (IsSimulated && !simulatorEnabled)
            throw new BusinessRuleException(
                "SIMULATOR_DISABLED",
                "Simulator disabled",
                "Simulators are disabled for this organization."
            );
        if (instrument is not null)
        {
            instrument.EnsureActive();
            EnsureCompatibleWithInstrument(instrument, IsSimulated);
        }
        else if (externalSystem is { Status: not ActiveStatus.Active })
        {
            throw new BusinessRuleException(
                "EXTERNAL_SYSTEM_INACTIVE",
                "External system inactive",
                "The operation requires an active external system."
            );
        }

        //if (
        //    !IsSimulated
        //    && adapterType.SupportsConnectionTest
        //    && !passedTestForCurrentConfiguration
        //)
        //    throw new BusinessRuleException(
        //        "CONNECTOR_TEST_REQUIRED",
        //        "Connection test required",
        //        "A passed connection test for the current configuration is required before enabling."
        //    );

        var passedForCurrentConfiguration =
       latestTestForCurrentConfiguration is { Outcome: ConnectorTestOutcome.Passed } t
       && t.ConnectorId == Id
       && t.ConfigVersion == ConfigVersion;
        if (!IsSimulated && adapterType.SupportsConnectionTest && !passedForCurrentConfiguration)
            throw new BusinessRuleException("CONNECTOR_TEST_REQUIRED", "Connection test required",
                "A passed connection test for the current configuration is required before enabling.");

        Status = ConnectorStatus.Enabled;
        EverEnabled = true;
        EnabledAt = now;
        DisabledAt = null;
        StampUpdated(now, actorId);
        Raise(new ConnectorEnabled(OrganizationId, Id, ConfigVersion, now));
    }

    public void Disable(DateTimeOffset now, Guid? actorId)
    {
        if (Status == ConnectorStatus.Disabled)
            return;
        if (Status != ConnectorStatus.Enabled)
            throw new BusinessRuleException(
                "CONNECTOR_INVALID_STATE",
                "Invalid connector state",
                "Only an enabled connector can be disabled."
            );

        Status = ConnectorStatus.Disabled;
        DisabledAt = now;
        StampUpdated(now, actorId);
        Raise(new ConnectorDisabled(OrganizationId, Id, now));
    }

    /// <summary>Only a Draft, never-enabled, unreferenced connector may be deleted (BR-CON-014).</summary>
    public void EnsureDeletable(bool isReferenced)
    {
        if (Status != ConnectorStatus.Draft || EverEnabled || isReferenced)
            throw new ConflictException(
                "CONNECTOR_IN_USE",
                "Connector in use",
                "Only a draft, never-enabled and unreferenced connector can be deleted."
            );
    }

    private void EnsureNotEnabled()
    {
        if (Status == ConnectorStatus.Enabled)
            throw new ConflictException(
                "CONNECTOR_ENABLED",
                "Connector enabled",
                "The change is not allowed while the connector is enabled."
            );
    }

    private ConnectorConfigRevision NewVersion(DateTimeOffset now, Guid? actorId, string reason)
    {
        ConfigVersion++;
        StampUpdated(now, actorId);
        return CreateRevision(now, actorId, reason);
    }

    //private ConnectorConfigRevision CreateRevision(DateTimeOffset now, Guid? actorId, string reason)
    //{
    //    var snapshot = new
    //    {
    //        direction = Direction.ToString(),
    //        instrumentId = InstrumentId,
    //        externalSystemId = ExternalSystemId,
    //        adapterTypeId = AdapterTypeId,
    //        profileVersionId = ProfileVersionId,
    //        isSimulated = IsSimulated,
    //        settings = JsonDocument.Parse(SettingsJson).RootElement,
    //        secretSlotVersions = _secretRefs.ToDictionary(s => s.Slot, s => s.Version),
    //    };
    //    return new ConnectorConfigRevision(
    //        OrganizationId,
    //        Id,
    //        ConfigVersion,
    //        JsonSerializer.Serialize(snapshot),
    //        now,
    //        actorId,
    //        reason
    //    );
    //}

    private ConnectorConfigRevision CreateRevision(DateTimeOffset now, Guid? actorId, string reason)
    {
        using var settings = JsonDocument.Parse(SettingsJson);
        var snapshot = new
        {
            direction = Direction.ToString(),
            instrumentId = InstrumentId,
            externalSystemId = ExternalSystemId,
            adapterTypeId = AdapterTypeId,
            profileVersionId = ProfileVersionId,
            isSimulated = IsSimulated,
            settings = settings.RootElement,
            secretSlotVersions = _secretRefs.ToDictionary(s => s.Slot, s => s.Version),
        };
        return new ConnectorConfigRevision(OrganizationId, Id, ConfigVersion,
            JsonSerializer.Serialize(snapshot), now, actorId, reason);
    }

    private static void EnsureCompatibleWithInstrument(
        Instrument instrument,
        bool connectorIsSimulated
    )
    {
        if (connectorIsSimulated)
            return;
        if (instrument.IsSimulated || instrument.CompatibilityClass != CompatibilityClass.C1)
            throw new BusinessRuleException(
                "CONNECTOR_CLASS_NOT_SUPPORTED",
                "Compatibility class not supported",
                "Non-simulated connectors may serve only real instruments assessed as class C1."
            );
    }

    private static string RequireJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                throw new JsonException();
        }
        catch (Exception ex) when (ex is JsonException or ArgumentException)
        {
            throw new ValidationException(
                "CONNECTOR_SETTINGS_INVALID",
                "Invalid settings",
                "The connector settings must be a JSON object."
            );
        }
        return json;
    }
}

using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.InstrumentManagement.Enums;

namespace LabBridge.Domain.Features.InstrumentManagement.Entities;

/// <summary>
/// Registered adapter type (platform reference data): the kind of transport/protocol adapter a connector uses.
/// Protocol adapter implementations are out of scope; this entity only describes capabilities.
/// </summary>
public sealed class AdapterType : Entity
{
    private readonly List<string> _supportedDirectionNames = [];

    private AdapterType() { }

    public string Code { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public string SettingsSchemaJson { get; private set; } = null!;
    public string SecretSlotsJson { get; private set; } = null!;
    public string? BindingAttributesJson { get; private set; }
    public bool SupportsConnectionTest { get; private set; }
    public bool IsSimulator { get; private set; }
    public string AdapterVersion { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<ConnectorDirection> SupportedDirections =>
        _supportedDirectionNames.Select(Enum.Parse<ConnectorDirection>).ToList().AsReadOnly();

    public static AdapterType Register(
        string code,
        string displayName,
        IEnumerable<ConnectorDirection> supportedDirections,
        string settingsSchemaJson,
        string secretSlotsJson,
        string? bindingAttributesJson,
        bool supportsConnectionTest,
        bool isSimulator,
        string adapterVersion,
        DateTimeOffset now
    )
    {
        var directions = supportedDirections.Distinct().ToList();
        if (directions.Count == 0)
            throw new ValidationException(
                "ADAPTER_DIRECTIONS_REQUIRED",
                "Supported directions required",
                "An adapter type must support at least one direction."
            );
        var adapter = new AdapterType
        {
            Id = Guid.NewGuid(),
            Code = Guard.Required(code, nameof(code), 100),
            DisplayName = Guard.Required(displayName, nameof(displayName), 200),
            SettingsSchemaJson = Guard.RequiredExact(
                settingsSchemaJson,
                nameof(settingsSchemaJson),
                int.MaxValue
            ),
            SecretSlotsJson = Guard.RequiredExact(
                secretSlotsJson,
                nameof(secretSlotsJson),
                int.MaxValue
            ),
            BindingAttributesJson = bindingAttributesJson,
            SupportsConnectionTest = supportsConnectionTest,
            IsSimulator = isSimulator,
            AdapterVersion = Guard.Required(adapterVersion, nameof(adapterVersion), 30),
            IsActive = true,
            CreatedAt = now,
        };
        adapter._supportedDirectionNames.AddRange(
            directions.Select(d => Guard.Defined(d, nameof(supportedDirections)).ToString())
        );
        return adapter;
    }

    public bool Supports(ConnectorDirection direction) =>
        _supportedDirectionNames.Contains(direction.ToString());

    public void Deactivate() => IsActive = false;
}

using LabBridge.Domain.Common;
using LabBridge.Domain.Features.OrganizationManagement.Enums;

namespace LabBridge.Domain.Features.OrganizationManagement.Entities;

/// <summary>Organization-specific value of a registered setting. Unique per (OrganizationId, SettingDefinitionId); versioned per change (BR-ORG-011).</summary>
public sealed class OrganizationSetting : AggregateRoot, IOrganizationOwned
{
    private OrganizationSetting() { }

    public Guid OrganizationId { get; private set; }
    public Guid SettingDefinitionId { get; private set; }
    public string ValueJson { get; private set; } = null!;
    public int Version { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public static OrganizationSetting Create(
        Guid organizationId,
        SettingDefinition definition,
        string valueJson,
        SettingEditorScope caller,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        ArgumentNullException.ThrowIfNull(definition);
        definition.EnsureEditableBy(caller);
        definition.ValidateValue(valueJson);
        return new OrganizationSetting
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guard.NotEmpty(organizationId, nameof(organizationId)),
            SettingDefinitionId = definition.Id,
            ValueJson = valueJson,
            Version = 1,
            CreatedAt = now,
            UpdatedAt = now,
            UpdatedBy = actorId,
        };
    }

    public void ChangeValue(
        SettingDefinition definition,
        string valueJson,
        SettingEditorScope caller,
        DateTimeOffset now,
        Guid? actorId
    )
    {
        ArgumentNullException.ThrowIfNull(definition);
        definition.EnsureEditableBy(caller);
        definition.ValidateValue(valueJson);
        if (valueJson == ValueJson)
            return;
        ValueJson = valueJson;
        Version++;
        UpdatedAt = now;
        UpdatedBy = actorId;
    }
}

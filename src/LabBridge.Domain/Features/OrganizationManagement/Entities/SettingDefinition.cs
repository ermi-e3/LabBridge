using System.Text.Json;
using LabBridge.Domain.Common;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.OrganizationManagement.Enums;

namespace LabBridge.Domain.Features.OrganizationManagement.Entities;

/// <summary>
/// Registered, typed, bounded setting definition (platform reference data, changed by migration only; BR-ORG-010).
/// ASSUMPTION A-ORG-01: ConstraintsJson uses the keys "min", "max" (Integer), "maxLength" (String) and "values" (Enum string array);
/// the FRS names the constraint kinds (range, enumeration, length) but does not fix the JSON shape.
/// </summary>
public sealed class SettingDefinition : Entity
{
    private SettingDefinition() { }

    public string Key { get; private set; } = null!;
    public SettingValueType ValueType { get; private set; }
    public string? ConstraintsJson { get; private set; }
    public string? DefaultValueJson { get; private set; }
    public string? PlatformMinJson { get; private set; }
    public string? PlatformMaxJson { get; private set; }
    public SettingEditableBy EditableBy { get; private set; }
    public string OwnerModule { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static SettingDefinition Define(
        string key,
        SettingValueType valueType,
        SettingEditableBy editableBy,
        string ownerModule,
        string description,
        DateTimeOffset now,
        string? constraintsJson = null,
        string? defaultValueJson = null,
        string? platformMinJson = null,
        string? platformMaxJson = null
    )
    {
        var definition = new SettingDefinition
        {
            Id = Guid.NewGuid(),
            Key = Guard.Required(key, nameof(key), 100),
            ValueType = Guard.Defined(valueType, nameof(valueType)),
            EditableBy = Guard.Defined(editableBy, nameof(editableBy)),
            OwnerModule = Guard.Required(ownerModule, nameof(ownerModule), 3).ToUpperInvariant(),
            Description = Guard.Required(description, nameof(description), 500),
            ConstraintsJson = constraintsJson,
            PlatformMinJson = platformMinJson,
            PlatformMaxJson = platformMaxJson,
            IsActive = true,
            CreatedAt = now,
        };

        definition.ValidateDefinition();   // CHANGE: fail at definition time, not when a value is first validated

        if (defaultValueJson is not null)
        {
            definition.ValidateValue(defaultValueJson);
            definition.DefaultValueJson = defaultValueJson;
        }
        return definition;
    }

    public void Deactivate() => IsActive = false;

    public void EnsureEditableBy(SettingEditorScope caller)
    {
        //var allowed = EditableBy == SettingEditableBy.Both || (int)EditableBy == (int)caller;
        var allowed = EditableBy switch
        {
            SettingEditableBy.Both => true,
            SettingEditableBy.Platform => caller == SettingEditorScope.Platform,
            SettingEditableBy.Organization => caller == SettingEditorScope.Organization,
            _ => false,
        };
        if (!allowed)
            throw new ForbiddenException(
                "SETTING_NOT_EDITABLE",
                "Setting not editable",
                "The setting cannot be changed from the caller's scope."
            );
    }

    /// <summary>Validates a JSON value against the type, constraints and platform bounds (VR-ORG-012/013/015).</summary>
    public void ValidateValue(string valueJson)
    {
        if (!IsActive)
            throw new ValidationException(
                "SETTING_UNKNOWN",
                "Unknown setting",
                "The setting is not registered or no longer active."
            );
        JsonElement value;
        try
        {
            using var doc = JsonDocument.Parse(valueJson);
            value = doc.RootElement.Clone();
        }
        catch (JsonException)
        {
            throw Invalid();
        }

        switch (ValueType)
        {
            case SettingValueType.Boolean
                when value.ValueKind is not (JsonValueKind.True or JsonValueKind.False):
            case SettingValueType.Integer
                when value.ValueKind != JsonValueKind.Number || !value.TryGetInt64(out _):
            case SettingValueType.String
            or SettingValueType.Enum when value.ValueKind != JsonValueKind.String:
                throw Invalid();
        }

        if (ValueType == SettingValueType.Integer)
        {
            var number = value.GetInt64();
            if (TryGetBound(ConstraintsJson, "min", out var min) && number < min)
                throw Invalid();
            if (TryGetBound(ConstraintsJson, "max", out var max) && number > max)
                throw Invalid();
            if (
                TryGetScalar(PlatformMinJson, out var pMin) && number < pMin
                || TryGetScalar(PlatformMaxJson, out var pMax) && number > pMax
            )
                throw new ValidationException(
                    "SETTING_OUT_OF_BOUNDS",
                    "Setting out of bounds",
                    "The value is outside the platform bounds."
                );
        }
        else if (
            ValueType == SettingValueType.String
            && TryGetBound(ConstraintsJson, "maxLength", out var maxLength)
            && value.GetString()!.Length > maxLength
        )
        {
            throw Invalid();
        }
        else if (ValueType == SettingValueType.Enum && !IsAllowedEnumValue(value.GetString()!))
        {
            throw Invalid();
        }
    }

    private bool IsAllowedEnumValue(string candidate)
    {
        if (ConstraintsJson is null)
            return true;
        using var doc = JsonDocument.Parse(ConstraintsJson);
        return !doc.RootElement.TryGetProperty("values", out var values)
            || values
                .EnumerateArray()
                .Any(v => v.ValueKind == JsonValueKind.String && v.GetString() == candidate);
    }

    private static bool TryGetBound(string? json, string name, out long result)
    {
        result = 0;
        if (json is null)
            return false;
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty(name, out var p) && p.TryGetInt64(out result);
    }

    private static bool TryGetScalar(string? json, out long result)
    {
        result = 0;
        if (json is null)
            return false;
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetInt64(out result);
    }

    /// <summary>Constraint documents must be valid JSON; enum settings must list their allowed values; bounds must be ordered.</summary>
    private void ValidateDefinition()
    {
        RequireValidJson(ConstraintsJson, nameof(ConstraintsJson));
        RequireValidJson(PlatformMinJson, nameof(PlatformMinJson));
        RequireValidJson(PlatformMaxJson, nameof(PlatformMaxJson));

        if (ValueType == SettingValueType.Enum && !HasEnumValues())
            throw new ValidationException(
                "SETTING_DEFINITION_INVALID", "Invalid setting definition", "An enum setting must define its allowed values.");

        if (TryGetScalar(PlatformMinJson, out var min) && TryGetScalar(PlatformMaxJson, out var max) && min > max)
            throw new ValidationException(
                "SETTING_DEFINITION_INVALID", "Invalid setting definition", "The platform minimum must not exceed the maximum.");
    }

    private bool HasEnumValues()
    {
        if (ConstraintsJson is null)
            return false;
        using var doc = JsonDocument.Parse(ConstraintsJson);
        return doc.RootElement.TryGetProperty("values", out var values)
            && values.ValueKind == JsonValueKind.Array
            && values.GetArrayLength() > 0;
    }

    private static void RequireValidJson(string? json, string field)
    {
        if (json is null)
            return;
        try
        {
            using var _ = JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            throw new ValidationException(
                "SETTING_DEFINITION_INVALID", "Invalid setting definition", $"Field {field} must contain valid JSON.");
        }
    }

    private static ValidationException Invalid() =>
        new(
            "SETTING_INVALID",
            "Invalid setting value",
            "The value does not satisfy the setting definition."
        );
}

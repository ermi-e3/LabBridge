using LabBridge.Domain.Common.Exceptions;
using System.Text.Json;


namespace LabBridge.Domain.Common;

/// <summary>Domain argument validation. Raises ValidationException with stable codes; details name the field, never the offending value.</summary>
public static class Guard
{
    public static Guid NotEmpty(Guid value, string field)
        => value != Guid.Empty ? value : throw Required(field);

    public static Guid? NotEmptyWhenSet(Guid? value, string field)
        => value is { } v && v == Guid.Empty ? throw Required(field) : value;

    public static string Required(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw Required(field);
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : throw TooLong(field, maxLength);
    }

    public static string? Optional(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : throw TooLong(field, maxLength);
    }

    /// <summary>Validates length without trimming (for opaque tokens, endpoints, reference identifiers).</summary>
    public static string RequiredExact(string? value, string field, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            throw Required(field);
        return value.Length <= maxLength ? value : throw TooLong(field, maxLength);
    }

    public static int InRange(int value, int min, int max, string field)
        => value >= min && value <= max ? value : throw new ValidationException(
            "VALIDATION_OUT_OF_RANGE", "Value out of range", $"Field {field} must be between {min} and {max}.");

    public static long NonNegative(long value, string field)
        => value >= 0 ? value : throw new ValidationException(
            "VALIDATION_OUT_OF_RANGE", "Value out of range", $"Field {field} must not be negative.");

    public static byte[] RequiredBytes(byte[]? value, string field)
        => value is { Length: > 0 } ? value : throw Required(field);

    public static T Defined<T>(T value, string field) where T : struct, Enum
        => Enum.IsDefined(value) ? value : throw new ValidationException(
            "VALIDATION_INVALID_VALUE", "Invalid value", $"Field {field} contains an undefined value.");

    private static ValidationException Required(string field)
        => new("VALIDATION_REQUIRED", "Required value missing", $"Field {field} is required.");

    private static ValidationException TooLong(string field, int max)
        => new("VALIDATION_TOO_LONG", "Value too long", $"Field {field} must not exceed {max} characters.");

    /// <summary>Returns null for blank input, otherwise validates that the text is JSON (so bad input fails as a domain error, not a database error).</summary>
    public static string? OptionalJson(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        try
        {
            using var _ = JsonDocument.Parse(value);
            return value;
        }
        catch (JsonException)
        {
            throw new ValidationException(
                "VALIDATION_INVALID_JSON", "Invalid JSON", $"Field {field} must contain valid JSON.");
        }
    }


    /// <summary>Validates that a required value is valid JSON, so bad input fails as a domain error and not as a database error.</summary>
    public static string RequiredJson(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw Required(field);
        try
        {
            using var _ = JsonDocument.Parse(value);
            return value;
        }
        catch (JsonException)
        {
            throw new ValidationException(
                "VALIDATION_INVALID_JSON", "Invalid JSON", $"Field {field} must contain valid JSON.");
        }
    }
}

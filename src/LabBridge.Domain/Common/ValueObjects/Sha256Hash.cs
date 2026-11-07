using System.Text.RegularExpressions;
using LabBridge.Domain.Common.Exceptions;

namespace LabBridge.Domain.Common.ValueObjects;

/// <summary>SHA-256 digest as 64 lowercase hexadecimal characters (content hashes of raw payloads, deduplication).</summary>
public sealed partial record Sha256Hash
{
    public const int Length = 64;

    public string Value { get; }

    private Sha256Hash(string value) => Value = value;

    public static Sha256Hash Create(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex) || !Pattern().IsMatch(hex.Trim()))
            throw new ValidationException("VALIDATION_HASH_INVALID", "Invalid content hash", "A SHA-256 hex digest of 64 characters is required.");
        return new Sha256Hash(hex.Trim().ToLowerInvariant());
    }

    [GeneratedRegex("^[0-9a-fA-F]{64}$", RegexOptions.CultureInvariant)]
    private static partial Regex Pattern();

    public override string ToString() => Value;
}

using System.Text.RegularExpressions;
using LabBridge.Domain.Common.Exceptions;

namespace LabBridge.Domain.Features.OrganizationManagement;

/// <summary>Stable human-readable organization code: 3-40 chars, [a-z0-9]+(-[a-z0-9]+)* (VR-ORG-001). Immutable after creation (BR-ORG-002).</summary>
public sealed partial record OrganizationCode
{
    public const int MinLength = 3;
    public const int MaxLength = 40;

    public string Value { get; }

    private OrganizationCode(string value) => Value = value;

    public static OrganizationCode Create(string? code)
    {
        var candidate = code?.Trim();
        if (string.IsNullOrEmpty(candidate) || candidate.Length is < MinLength or > MaxLength || !Pattern().IsMatch(candidate))
            throw new ValidationException("ORGANIZATION_CODE_INVALID", "Invalid organization code",
                "The organization code must be 3-40 characters of lowercase letters, digits and single hyphens.");
        return new OrganizationCode(candidate);
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex Pattern();

    public override string ToString() => Value;
}

using System.Text.RegularExpressions;
using LabBridge.Domain.Common.Exceptions;

namespace LabBridge.Domain.Common.ValueObjects;

/// <summary>E-mail address with its normalized form (lower(trim(email)); uniqueness is platform-wide, BR-IAM-016).</summary>
public sealed partial record EmailAddress
{
    public const int MaxLength = 320;

    public string Value { get; }
    public string Normalized { get; }

    private EmailAddress(string value, string normalized)
    {
        Value = value;
        Normalized = normalized;
    }

    public static EmailAddress Create(string? email)
    {
        var trimmed = Guard.Required(email, "email", MaxLength);
        if (!Pattern().IsMatch(trimmed))
            throw new ValidationException("USER_EMAIL_INVALID", "Invalid e-mail address", "The e-mail address is not in a valid format.");
        return new EmailAddress(trimmed, trimmed.ToLowerInvariant());
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)]
    private static partial Regex Pattern();

    public override string ToString() => Value;
}

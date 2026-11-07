namespace LabBridge.Domain.Common.Exceptions;

/// <summary>
/// Represents an attempt to create a resource or business record that must be unique.
/// </summary>
public sealed class DuplicateResourceException(
    string code,
    string title,
    string detail)
    : AppException(code, title, detail, 409);

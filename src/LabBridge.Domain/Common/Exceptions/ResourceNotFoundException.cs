namespace LabBridge.Domain.Common.Exceptions;

/// <summary>
/// Represents a required resource that does not exist (or is outside the caller tenant, which is indistinguishable by design).
/// </summary>
public sealed class ResourceNotFoundException(
    string code,
    string title,
    string detail)
    : AppException(code, title, detail, 404);

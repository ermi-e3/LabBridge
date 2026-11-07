namespace LabBridge.Domain.Common.Exceptions;

/// <summary>
/// Represents a conflict between an attempted operation and the current state of a resource or the system.
/// </summary>
public sealed class ConflictException(
    string code,
    string title,
    string detail)
    : AppException(code, title, detail, 409);

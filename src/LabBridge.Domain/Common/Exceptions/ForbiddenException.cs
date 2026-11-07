namespace LabBridge.Domain.Common.Exceptions;

/// <summary>
/// Represents an authenticated actor attempting an operation that is not permitted.
/// </summary>
public sealed class ForbiddenException(
    string code,
    string title,
    string detail)
    : AppException(code, title, detail, 403);

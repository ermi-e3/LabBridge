namespace LabBridge.Domain.Common.Exceptions;

/// <summary>
/// Represents an expected authentication failure that is part of the documented contract.
/// </summary>
public sealed class AuthenticationException(
    string code,
    string title,
    string detail)
    : AppException(code, title, detail, 401);

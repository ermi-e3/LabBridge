namespace LabBridge.Domain.Common.Exceptions;

/// <summary>
/// Represents invalid domain input (invalid code, value, configuration or missing mandatory value).
/// </summary>
public sealed class ValidationException(
    string code,
    string title,
    string detail)
    : AppException(code, title, detail, 400);

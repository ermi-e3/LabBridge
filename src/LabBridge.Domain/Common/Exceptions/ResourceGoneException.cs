namespace LabBridge.Domain.Common.Exceptions;

/// <summary>
/// Represents a resource that existed but is no longer available according to its documented lifecycle or retention rules.
/// </summary>
public sealed class ResourceGoneException(
    string code,
    string title,
    string detail)
    : AppException(code, title, detail, 410);

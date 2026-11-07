namespace LabBridge.Domain.Common.Exceptions;

/// <summary>
/// Represents a concurrency conflict: the resource changed since it was read. The persistence boundary may translate EF Core concurrency failures into this exception.
/// </summary>
public sealed class ConcurrencyException(
    string code,
    string title,
    string detail)
    : AppException(code, title, detail, 409);

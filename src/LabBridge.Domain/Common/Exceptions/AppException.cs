namespace LabBridge.Domain.Common.Exceptions;

/// <summary>
/// Base exception for expected application failures.
/// The API global exception handler maps this exception to ProblemDetails.
/// </summary>
public abstract class AppException(
    string code,
    string title,
    string detail,
    int status)
    : Exception(detail)
{
    public string Code { get; } = code;

    public string Title { get; } = title;

    public int Status { get; } = status;
}

namespace LabBridge.Domain.Common.Exceptions;

/// <summary>
/// Represents a violation of a documented domain business rule (invalid state transition, operation not allowed in the current state).
/// </summary>
public sealed class BusinessRuleException(
    string code,
    string title,
    string detail)
    : AppException(code, title, detail, 409);

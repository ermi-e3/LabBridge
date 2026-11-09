using LabBridge.Domain.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LabBridge.Api.ExceptionHandlers;

/// <summary>
/// Centralized exception handler for the LabBridge API.
///
/// Converts known application exceptions into standardized
/// ProblemDetails responses and handles unexpected exceptions
/// without exposing internal implementation details.
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // The client has disconnected. There is no useful response
        // to write, so avoid unnecessary logging and processing.
        if (exception is OperationCanceledException &&
            httpContext.RequestAborted.IsCancellationRequested)
        {
            return true;
        }

        var problemDetails = CreateProblemDetails(
            httpContext,
            exception);

        LogException(exception, httpContext);

        httpContext.Response.StatusCode =
            problemDetails.Status!.Value;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        Exception exception)
    {
        var traceId = Activity.Current?.Id
                      ?? httpContext.TraceIdentifier;

        var problemDetails = exception is AppException appException
            ? new ProblemDetails
            {
                Status = appException.Status,
                Title = appException.Title,
                Detail = appException.Message,
                Type = "about:blank",
                Instance = httpContext.Request.Path
            }
            : new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = "An unexpected error occurred while processing your request.",
                Type = "about:blank",
                Instance = httpContext.Request.Path
            };

        // Add machine-readable information for API consumers.
        problemDetails.Extensions["code"] =
            exception is AppException knownException
                ? knownException.Code
                : "INTERNAL_SERVER_ERROR";

        // Allows developers to correlate client errors with server logs.
        problemDetails.Extensions["traceId"] = traceId;

        return problemDetails;
    }

    private void LogException(
        Exception exception,
        HttpContext httpContext)
    {
        if (exception is AppException)
        {
            // Expected application failures are not necessarily
            // system errors, so log them as warnings.
            logger.LogWarning(
                "Application exception occurred. " +
                "Method: {Method}, Path: {Path}, Exception: {ExceptionType}, Message: {Message}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                exception.GetType().Name,
                exception.Message);

            return;
        }

        // Unexpected exceptions indicate potentially unhandled
        // application or infrastructure failures.
        logger.LogError(
            exception,
            "Unhandled exception occurred. " +
            "Method: {Method}, Path: {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);
    }
}

using LabBridge.Application.Common.Idempotency;
using LabBridge.Application.Common.Organizations;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Common.Idempotency;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace LabBridge.Api.Idempotency;

public sealed class IdempotencyMiddleware(
    RequestDelegate next,
    ILogger<IdempotencyMiddleware> logger)
{
    private static readonly HashSet<string> ReplayableHeaders =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Location",
            "ETag",
            "Content-Language"
        };

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(
        HttpContext context,
        IIdempotencyService idempotencyService,
        ICurrentOrganization currentOrganization,
        IOptions<IdempotencyOptions> options)
    {
        var metadata = context.GetEndpoint()?
            .Metadata.GetMetadata<IdempotentAttribute>();

        if (metadata is null)
        {
            await next(context);
            return;
        }

        if (!IsMutationMethod(context.Request.Method))
        {
            throw new ValidationException(
                "IDEMPOTENCY_METHOD_NOT_SUPPORTED",
                "HTTP method not supported",
                "Idempotency can only be used with mutation endpoints.");
        }

        var settings = options.Value;

        var key = context.Request.Headers[
            settings.HeaderName].ToString();

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ValidationException(
                "IDEMPOTENCY_KEY_REQUIRED",
                "Idempotency key required",
                $"The {settings.HeaderName} request header is required.");
        }

        key = key.Trim();

        if (key.Length is < 16 or > 128)
        {
            throw new ValidationException(
                "IDEMPOTENCY_KEY_INVALID",
                "Invalid idempotency key",
                "The idempotency key must contain between 16 and 128 characters.");
        }

        // Resolve tenant ownership only from authenticated server-side context.
        Guid? organizationId;

        if (metadata.OrganizationScoped)
        {
            organizationId = currentOrganization.OrganizationId;

            if (organizationId is null)
            {
                throw new ForbiddenException(
                    "ORGANIZATION_CONTEXT_REQUIRED",
                    "Organization context required",
                    "This operation requires an authenticated organization context.");
            }
        }
        else
        {
            // Platform-level operations must not accidentally execute
            // using an organization-scoped identity.
            if (currentOrganization.OrganizationId is not null)
            {
                throw new ForbiddenException(
                    "PLATFORM_CONTEXT_REQUIRED",
                    "Platform context required",
                    "This operation is available only in a platform-level context.");
            }

            organizationId = null;
        }

        var operationScope = metadata.OperationScope;

        if (string.IsNullOrWhiteSpace(operationScope) ||
            operationScope.Length > 256)
        {
            throw new InvalidOperationException(
                "The configured idempotency operation scope is invalid.");
        }

        var requestBody = await ReadRequestBodyAsync(
            context,
            settings.MaxRequestBodyBytes);

        var fingerprint = RequestFingerprint.Create(
            context.Request.Method,
            context.Request.Path.Value ?? "/",
            context.Request.QueryString.Value ?? string.Empty,
            context.Request.ContentType,
            requestBody);

        var now = DateTimeOffset.UtcNow;

        var reservation = IdempotencyRecord.Reserve(
            organizationId,
            operationScope,
            key,
            fingerprint,
            now,
            settings.Retention);

        var reserved = await idempotencyService.TryReserveAsync(
            reservation,
            context.RequestAborted);

        if (!reserved)
        {
            var existing = await idempotencyService.FindAsync(
                organizationId,
                operationScope,
                key,
                context.RequestAborted);

            if (existing is null)
            {
                // A concurrent expiration/reclamation may have changed
                // the record between reservation and lookup.
                // Retry once with a fresh reservation.
                reservation = IdempotencyRecord.Reserve(
                    organizationId,
                    operationScope,
                    key,
                    fingerprint,
                    DateTimeOffset.UtcNow,
                    settings.Retention);

                reserved = await idempotencyService.TryReserveAsync(
                    reservation,
                    context.RequestAborted);

                if (!reserved)
                {
                    existing = await idempotencyService.FindAsync(
                        organizationId,
                        operationScope,
                        key,
                        context.RequestAborted);
                }
            }

            if (!reserved)
            {
                if (existing is null)
                {
                    throw new ConflictException(
                        "IDEMPOTENCY_RESERVATION_CONFLICT",
                        "Idempotency reservation conflict",
                        "The request could not obtain an idempotency reservation. Please retry.");
                }

                existing.EnsureSameRequest(fingerprint);

                if (existing.Status == IdempotencyStatus.InProgress)
                {
                    context.Response.Headers.RetryAfter = "1";

                    throw new ConflictException(
                        "IDEMPOTENCY_REQUEST_IN_PROGRESS",
                        "Request already processing",
                        "A request with this idempotency key is still being processed. Retry shortly.");
                }

                if (existing.Status == IdempotencyStatus.Completed)
                {
                    await ReplayResponseAsync(context, existing);
                    return;
                }
            }
        }

        // This request owns the reservation.
        await ExecuteAndCaptureAsync(
            context,
            idempotencyService,
            reservation,
            settings.MaxResponseBodyBytes);
    }

    //private async Task ExecuteAndCaptureAsync(
    //    HttpContext context,
    //    IIdempotencyService service,
    //    IdempotencyRecord reservation,
    //    int maxResponseBodyBytes)
    //{
    //    var originalBody = context.Response.Body;

    //    await using var responseBuffer = new MemoryStream();

    //    context.Response.Body = responseBuffer;

    //    try
    //    {
    //        await next(context);

    //        var responseBytes = responseBuffer.ToArray();

    //        var successful = context.Response.StatusCode is >= 200 and <= 299;

    //        var isWithinLimit = responseBytes.Length <= maxResponseBodyBytes;

    //        if (successful && isWithinLimit)
    //        {
    //            var responseBody = Encoding.UTF8.GetString(responseBytes);

    //            var headers = GetReplayableHeaders(context.Response);

    //            var serializedHeaders = JsonSerializer.Serialize(
    //                headers,
    //                JsonOptions);

    //            await service.CompleteAsync(
    //                reservation,
    //                context.Response.StatusCode,
    //                responseBody,
    //                context.Response.ContentType,
    //                serializedHeaders,
    //                context.RequestAborted);
    //        }
    //        else
    //        {
    //            // Do not cache unsuccessful or oversized responses.
    //            await service.ReleaseAsync(
    //                reservation,
    //                CancellationToken.None);

    //            if (!successful)
    //            {
    //                logger.LogDebug(
    //                    "Idempotency reservation released because response status was {StatusCode}.",
    //                    context.Response.StatusCode);
    //            }
    //            else
    //            {
    //                logger.LogWarning(
    //                    "Idempotency response was not stored because it exceeded {MaxBytes} bytes.",
    //                    maxResponseBodyBytes);
    //            }
    //        }

    //        context.Response.Body = originalBody;

    //        responseBuffer.Position = 0;

    //        await responseBuffer.CopyToAsync(
    //            originalBody,
    //            context.RequestAborted);
    //    }
    //    catch
    //    {
    //        context.Response.Body = originalBody;

    //        // Best-effort cleanup. Do not allow cleanup failure to hide
    //        // the original exception.
    //        try
    //        {
    //            await service.ReleaseAsync(
    //                reservation,
    //                CancellationToken.None);
    //        }
    //        catch (Exception cleanupException)
    //        {
    //            logger.LogError(
    //                cleanupException,
    //                "Failed to release idempotency reservation {ReservationToken}.",
    //                reservation.ReservationToken);
    //        }

    //        throw;
    //    }
    //    finally
    //    {
    //        context.Response.Body = originalBody;
    //    }
    //}

    private async Task ExecuteAndCaptureAsync(
    HttpContext context,
    IIdempotencyService service,
    IdempotencyRecord reservation,
    int maxResponseBodyBytes)
    {
        var originalBody = context.Response.Body;

        await using var responseBuffer = new MemoryStream();

        context.Response.Body = responseBuffer;

        var transactionStarted = false;

        try
        {
            // Begin the transaction after the reservation has been persisted.
            await service.BeginExecutionAsync(context.RequestAborted);
            transactionStarted = true;

            // Controller and application-service database changes now
            // participate in the same scoped DbContext transaction.
            await next(context);

            var responseBytes = responseBuffer.ToArray();

            var successful =
                context.Response.StatusCode is >= 200 and <= 299;

            if (successful)
            {
                // Never commit business changes without storing the replay response.
                if (responseBytes.Length > maxResponseBodyBytes)
                {
                    throw new InvalidOperationException(
                        "The idempotency response exceeded the configured response size limit. " +
                        "The database transaction will be rolled back.");
                }

                var responseBody = Encoding.UTF8.GetString(responseBytes);

                var headers = GetReplayableHeaders(context.Response);

                var serializedHeaders = JsonSerializer.Serialize(
                    headers,
                    JsonOptions);

                // This database update participates in the active transaction.
                await service.CompleteAsync(
                    reservation,
                    context.Response.StatusCode,
                    responseBody,
                    context.Response.ContentType,
                    serializedHeaders,
                    context.RequestAborted);

                // Business data and the completed response commit together.
                await service.CommitExecutionAsync(context.RequestAborted);
                transactionStarted = false;
            }
            else
            {
                // Do not commit business writes for an unsuccessful response.
                await service.RollbackExecutionAsync(CancellationToken.None);
                transactionStarted = false;

                await service.ReleaseAsync(
                    reservation,
                    CancellationToken.None);
            }

            context.Response.Body = originalBody;

            responseBuffer.Position = 0;

            await responseBuffer.CopyToAsync(
                originalBody,
                context.RequestAborted);
        }
        catch
        {
            context.Response.Body = originalBody;

            if (transactionStarted)
            {
                try
                {
                    await service.RollbackExecutionAsync(
                        CancellationToken.None);
                }
                catch (Exception rollbackException)
                {
                    logger.LogError(
                        rollbackException,
                        "Failed to roll back idempotency execution transaction.");
                }
            }

            try
            {
                await service.ReleaseAsync(
                    reservation,
                    CancellationToken.None);
            }
            catch (Exception cleanupException)
            {
                logger.LogError(
                    cleanupException,
                    "Failed to release idempotency reservation {ReservationToken}.",
                    reservation.ReservationToken);
            }

            throw;
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }

    private static async Task<byte[]> ReadRequestBodyAsync(
        HttpContext context,
        int maxBytes)
    {
        context.Request.EnableBuffering();

        await using var buffer = new MemoryStream();

        var body = context.Request.Body;
        var chunk = new byte[81920];
        var total = 0;

        while (true)
        {
            var read = await body.ReadAsync(
                chunk.AsMemory(0, chunk.Length),
                context.RequestAborted);

            if (read == 0)
                break;

            total += read;

            if (total > maxBytes)
            {
                throw new ValidationException(
                    "IDEMPOTENCY_REQUEST_TOO_LARGE",
                    "Request too large",
                    "The request body exceeds the configured idempotency fingerprinting limit.");
            }

            await buffer.WriteAsync(
                chunk.AsMemory(0, read),
                context.RequestAborted);
        }

        context.Request.Body.Position = 0;

        return buffer.ToArray();
    }

    private static Dictionary<string, string[]> GetReplayableHeaders(
        HttpResponse response)
    {
        var headers = new Dictionary<string, string[]>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var name in ReplayableHeaders)
        {
            if (response.Headers.TryGetValue(name, out var values))
                headers[name] = values.ToArray();
        }

        return headers;
    }

    private static async Task ReplayResponseAsync(
        HttpContext context,
        IdempotencyRecord record)
    {
        if (record.ResponseStatus is null)
        {
            throw new InvalidOperationException(
                "A completed idempotency record must have a response status.");
        }

        context.Response.StatusCode = record.ResponseStatus.Value;

        if (!string.IsNullOrWhiteSpace(record.ResponseContentType))
            context.Response.ContentType = record.ResponseContentType;

        if (!string.IsNullOrWhiteSpace(record.ResponseHeaders))
        {
            var headers =
                JsonSerializer.Deserialize<Dictionary<string, string[]>>(
                    record.ResponseHeaders,
                    JsonOptions);

            if (headers is not null)
            {
                foreach (var header in headers)
                {
                    if (ReplayableHeaders.Contains(header.Key))
                    {
                        context.Response.Headers[header.Key] = header.Value;
                    }
                }
            }
        }

        context.Response.Headers["Idempotency-Replayed"] = "true";

        if (!string.IsNullOrEmpty(record.ResponseBody))
        {
            await context.Response.WriteAsync(
                record.ResponseBody,
                context.RequestAborted);
        }
    }

    private static bool IsMutationMethod(string method)
    {
        return HttpMethods.IsPost(method)
            || HttpMethods.IsPut(method)
            || HttpMethods.IsPatch(method)
            || HttpMethods.IsDelete(method);
    }
}
using LabBridge.Domain.Common.Idempotency;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabBridge.Application.Common.Idempotency;

//public interface IIdempotencyService
//{
//    Task<IdempotencyRecord?> FindAsync(Guid? organizationId, string operationScope, string key, CancellationToken cancellationToken);

//    Task<bool> TryReserveAsync(IdempotencyRecord record, CancellationToken cancellationToken);

//    Task CompleteAsync(IdempotencyRecord record,int responseStatus, string? responseBody, string? contentType, string? headers, CancellationToken cancellationToken);
//}


public interface IIdempotencyService
{
    Task<IdempotencyRecord?> FindAsync(
        Guid? organizationId,
        string operationScope,
        string key,
        CancellationToken cancellationToken);

    /// <summary>
    /// Atomically creates a reservation or reclaims an expired reservation.
    /// Returns false if an unexpired reservation already exists.
    /// </summary>
    Task<bool> TryReserveAsync(
        IdempotencyRecord record,
        CancellationToken cancellationToken);

    /// <summary>
    /// Completes the reservation only when the supplied reservation token
    /// still owns the current reservation.
    /// </summary>
    Task CompleteAsync(
        IdempotencyRecord record,
        int responseStatus,
        string? responseBody,
        string? contentType,
        string? headers,
        CancellationToken cancellationToken);

    /// <summary>
    /// Removes an unsuccessful reservation only if this request still owns it.
    /// </summary>
    Task ReleaseAsync(
        IdempotencyRecord record,
        CancellationToken cancellationToken);

    /// <summary>
    /// Deletes expired records in batches.
    /// </summary>
    Task<int> DeleteExpiredAsync(
        DateTimeOffset now,
        int batchSize,
        CancellationToken cancellationToken);

    Task BeginExecutionAsync(CancellationToken cancellationToken);

    Task CommitExecutionAsync(CancellationToken cancellationToken);

    Task RollbackExecutionAsync(CancellationToken cancellationToken);
}
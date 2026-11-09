using LabBridge.Application.Common.Idempotency;
using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Common.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Npgsql;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace LabBridge.Infrastructure.Common;

//public sealed class IdempotencyService(
//    LabBridgeDbContext dbContext) : IIdempotencyService
//{
//    public async Task<IdempotencyRecord?> FindAsync(
//        Guid? organizationId,
//        string operationScope,
//        string key,
//        CancellationToken cancellationToken)
//    {
//        return await dbContext.Set<IdempotencyRecord>()
//            .AsNoTracking()
//            .SingleOrDefaultAsync(
//                x => x.OrganizationId == organizationId
//                     && x.OperationScope == operationScope
//                     && x.Key == key,
//                cancellationToken);
//    }

//    public async Task<bool> TryReserveAsync(
//        IdempotencyRecord record,
//        CancellationToken cancellationToken)
//    {
//        ArgumentNullException.ThrowIfNull(record);

//        const string organizationSql = """
//            INSERT INTO common.idempotency_records
//            (
//                id,
//                organization_id,
//                operation_scope,
//                key,
//                fingerprint,
//                status,
//                response_status,
//                response_body,
//                response_content_type,
//                response_headers,
//                created_at,
//                expires_at
//            )
//            VALUES
//            (
//                @id,
//                @organization_id,
//                @operation_scope,
//                @key,
//                @fingerprint,
//                @status,
//                NULL,
//                NULL,
//                NULL,
//                NULL,
//                @created_at,
//                @expires_at
//            )
//            ON CONFLICT (organization_id, operation_scope, key)
//                WHERE organization_id IS NOT NULL
//            DO UPDATE SET
//                key = EXCLUDED.key,
//                fingerprint = EXCLUDED.fingerprint,
//                status = EXCLUDED.status,
//                response_status = NULL,
//                response_body = NULL,
//                response_content_type = NULL,
//                response_headers = NULL,
//                created_at = EXCLUDED.created_at,
//                expires_at = EXCLUDED.expires_at
//            WHERE common.idempotency_records.expires_at <= EXCLUDED.created_at
//            RETURNING id;
//            """;

//        const string platformSql = """
//            INSERT INTO common.idempotency_records
//            (
//                id,
//                organization_id,
//                operation_scope,
//                key,
//                fingerprint,
//                status,
//                response_status,
//                response_body,
//                response_content_type,
//                response_headers,
//                created_at,
//                expires_at
//            )
//            VALUES
//            (
//                @id,
//                NULL,
//                @operation_scope,
//                @key,
//                @fingerprint,
//                @status,
//                NULL,
//                NULL,
//                NULL,
//                NULL,
//                @created_at,
//                @expires_at
//            )
//            ON CONFLICT (operation_scope, key)
//                WHERE organization_id IS NULL
//            DO UPDATE SET
//                fingerprint = EXCLUDED.fingerprint,
//                status = EXCLUDED.status,
//                response_status = NULL,
//                response_body = NULL,
//                response_content_type = NULL,
//                response_headers = NULL,
//                created_at = EXCLUDED.created_at,
//                expires_at = EXCLUDED.expires_at
//            WHERE common.idempotency_records.expires_at <= EXCLUDED.created_at
//            RETURNING id;
//            """;

//        var sql = record.OrganizationId.HasValue
//            ? organizationSql
//            : platformSql;

//        return await ExecuteReservationAsync(
//            sql,
//            record,
//            cancellationToken);
//    }

//    public async Task CompleteAsync(
//        IdempotencyRecord record,
//        int responseStatus,
//        string? responseBody,
//        string? contentType,
//        string? headers,
//        CancellationToken cancellationToken)
//    {
//        ArgumentNullException.ThrowIfNull(record);

//        if (responseStatus is < 100 or > 599)
//        {
//            throw new ArgumentOutOfRangeException(
//                nameof(responseStatus),
//                "The HTTP response status must be between 100 and 599.");
//        }

//        var affectedRows = await dbContext.Set<IdempotencyRecord>()
//            .Where(x =>
//                x.Id == record.Id &&
//                x.Status == IdempotencyStatus.InProgress &&
//                x.Fingerprint == record.Fingerprint)
//            .ExecuteUpdateAsync(setters => setters
//                .SetProperty(
//                    x => x.Status,
//                    IdempotencyStatus.Completed)
//                .SetProperty(
//                    x => x.ResponseStatus,
//                    (int?)responseStatus)
//                .SetProperty(
//                    x => x.ResponseBody,
//                    responseBody)
//                .SetProperty(
//                    x => x.ResponseContentType,
//                    contentType)
//                .SetProperty(
//                    x => x.ResponseHeaders,
//                    headers),
//                cancellationToken);

//        if (affectedRows == 0)
//        {
//            throw new ConflictException(
//                "IDEMPOTENCY_COMPLETION_FAILED",
//                "Idempotency completion failed",
//                "The idempotency record was not found, was already completed, or no longer matches the original request.");
//        }
//    }

//    private async Task<bool> ExecuteReservationAsync(
//        string sql,
//        IdempotencyRecord record,
//        CancellationToken cancellationToken)
//    {
//        var connection = dbContext.Database.GetDbConnection();
//        var shouldCloseConnection = connection.State != ConnectionState.Open;

//        if (shouldCloseConnection)
//        {
//            await connection.OpenAsync(cancellationToken);
//        }

//        try
//        {
//            await using var command = connection.CreateCommand();

//            command.CommandText = sql;

//            // Participate in the current EF Core transaction, if one exists.
//            var currentTransaction = dbContext.Database.CurrentTransaction;

//            if (currentTransaction is not null)
//            {
//                command.Transaction = currentTransaction.GetDbTransaction();
//            }

//            AddParameter(command, "@id", DbType.Guid, record.Id);
//            AddParameter(command, "@organization_id", DbType.Guid,
//                record.OrganizationId);
//            AddParameter(command, "@operation_scope", DbType.String,
//                record.OperationScope);
//            AddParameter(command, "@key", DbType.String, record.Key);
//            AddParameter(command, "@fingerprint", DbType.String,
//                record.Fingerprint);
//            AddParameter(command, "@status", DbType.String,
//                record.Status.ToString());
//            AddParameter(command, "@created_at", DbType.DateTimeOffset,
//                record.CreatedAt);
//            AddParameter(command, "@expires_at", DbType.DateTimeOffset,
//                record.ExpiresAt);

//            var result = await command.ExecuteScalarAsync(cancellationToken);

//            // PostgreSQL returns the inserted or reclaimed record ID.
//            // No returned ID means an unexpired conflicting record exists.
//            return result is Guid;
//        }
//        finally
//        {
//            if (shouldCloseConnection)
//            {
//                await connection.CloseAsync();
//            }
//        }
//    }

//    private static void AddParameter(
//        DbCommand command,
//        string name,
//        DbType type,
//        object? value)
//    {
//        var parameter = command.CreateParameter();

//        parameter.ParameterName = name;
//        parameter.DbType = type;
//        parameter.Value = value ?? DBNull.Value;

//        command.Parameters.Add(parameter);
//    }
//}


//public sealed class IdempotencyService : IIdempotencyService
public sealed class IdempotencyService(LabBridgeDbContext _dbContext) : IIdempotencyService
{
    private IDbContextTransaction? _executionTransaction;

    public async Task<IdempotencyRecord?> FindAsync(
        Guid? organizationId,
        string operationScope,
        string key,
        CancellationToken cancellationToken)
    {
        return await _dbContext.IdempotencyRecords
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId
                     && x.OperationScope == operationScope
                     && x.Key == key,
                cancellationToken);
    }

    public async Task<bool> TryReserveAsync(
        IdempotencyRecord record,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(record);

        var records = _dbContext.IdempotencyRecords;

        // First, atomically reclaim an expired reservation.
        // The reservation token changes, so an older request cannot complete it.
        var reclaimed = await records
            .Where(x =>
                x.OrganizationId == record.OrganizationId &&
                x.OperationScope == record.OperationScope &&
                x.Key == record.Key &&
                x.ExpiresAt <= record.CreatedAt)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Fingerprint, record.Fingerprint)
                .SetProperty(x => x.Status, IdempotencyStatus.InProgress)
                .SetProperty(x => x.ReservationToken, record.ReservationToken)
                .SetProperty(x => x.ResponseStatus, (int?)null)
                .SetProperty(x => x.ResponseBody, (string?)null)
                .SetProperty(x => x.ResponseContentType, (string?)null)
                .SetProperty(x => x.ResponseHeaders, (string?)null)
                .SetProperty(x => x.CreatedAt, record.CreatedAt)
                .SetProperty(x => x.ExpiresAt, record.ExpiresAt),
                cancellationToken);

        if (reclaimed > 0)
            return true;

        // Check whether an unexpired reservation exists.
        var exists = await records.AnyAsync(
            x => x.OrganizationId == record.OrganizationId
                 && x.OperationScope == record.OperationScope
                 && x.Key == record.Key,
            cancellationToken);

        if (exists)
            return false;

        // Attempt to insert a new reservation.
        await records.AddAsync(record, cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception)
            when (IsIdempotencyUniqueViolation(exception))
        {
            // Detach the failed entity so it does not remain in the DbContext.
            _dbContext.Entry(record).State = EntityState.Detached;
            return false;
        }
    }

    public async Task CompleteAsync(
        IdempotencyRecord record,
        int responseStatus,
        string? responseBody,
        string? contentType,
        string? headers,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(record);

        if (responseStatus is < 200 or > 299)
            throw new ArgumentOutOfRangeException(
                nameof(responseStatus),
                "Only successful HTTP responses can be stored.");

        // ReservationToken, not Id, identifies the current owner.
        var affectedRows = await _dbContext.IdempotencyRecords
            .Where(x =>
                x.OrganizationId == record.OrganizationId &&
                x.OperationScope == record.OperationScope &&
                x.Key == record.Key &&
                x.Fingerprint == record.Fingerprint &&
                x.ReservationToken == record.ReservationToken &&
                x.Status == IdempotencyStatus.InProgress)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, IdempotencyStatus.Completed)
                .SetProperty(x => x.ResponseStatus, (int?)responseStatus)
                .SetProperty(x => x.ResponseBody, responseBody)
                .SetProperty(x => x.ResponseContentType, contentType)
                .SetProperty(x => x.ResponseHeaders, headers),
                cancellationToken);

        if (affectedRows == 0)
        {
            throw new ConflictException(
                "IDEMPOTENCY_COMPLETION_FAILED",
                "Idempotency completion failed",
                "The reservation is no longer owned by this request.");
        }
    }

    public async Task ReleaseAsync(
        IdempotencyRecord record,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(record);

        await _dbContext.IdempotencyRecords
            .Where(x =>
                x.OrganizationId == record.OrganizationId &&
                x.OperationScope == record.OperationScope &&
                x.Key == record.Key &&
                x.ReservationToken == record.ReservationToken &&
                x.Status == IdempotencyStatus.InProgress)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<int> DeleteExpiredAsync(
        DateTimeOffset now,
        int batchSize,
        CancellationToken cancellationToken)
    {
        if (batchSize is < 1 or > 10_000)
            throw new ArgumentOutOfRangeException(nameof(batchSize));

        // Delete a bounded batch instead of loading every expired record.
        var expiredIds = await _dbContext.IdempotencyRecords
            .Where(x => x.ExpiresAt <= now)
            .OrderBy(x => x.ExpiresAt)
            .Select(x => x.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        if (expiredIds.Count == 0)
            return 0;

        return await _dbContext.IdempotencyRecords
            .Where(x => expiredIds.Contains(x.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }

    private static bool IsIdempotencyUniqueViolation(
        DbUpdateException exception)
    {
        var postgresException = GetPostgresException(exception);

        if (postgresException is null ||
            postgresException.SqlState != PostgresErrorCodes.UniqueViolation)
        {
            return false;
        }

        return postgresException.ConstraintName is
            "ux_idempotency_org_scope_key" or
            "ux_idempotency_platform_scope_key";
    }

    private static PostgresException? GetPostgresException(
        Exception exception)
    {
        for (var current = exception;
             current is not null;
             current = current.InnerException!)
        {
            if (current is PostgresException postgresException)
                return postgresException;
        }

        return null;
    }

    public async Task BeginExecutionAsync(
    CancellationToken cancellationToken)
    {
        if (_executionTransaction is not null)
            throw new InvalidOperationException(
                "An idempotency execution transaction is already active.");

        _executionTransaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitExecutionAsync(
        CancellationToken cancellationToken)
    {
        var transaction = _executionTransaction
            ?? throw new InvalidOperationException(
                "No idempotency execution transaction is active.");

        try
        {
            await transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await transaction.DisposeAsync();
            _executionTransaction = null;
        }
    }

    public async Task RollbackExecutionAsync(
        CancellationToken cancellationToken)
    {
        var transaction = _executionTransaction;

        if (transaction is null)
            return;

        try
        {
            await transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await transaction.DisposeAsync();
            _executionTransaction = null;
        }
    }
}
using LabBridge.Application.Common.Idempotency;
using Microsoft.Extensions.Options;

namespace LabBridge.Api.Idempotency;

public sealed class IdempotencyCleanupService(
    IServiceScopeFactory scopeFactory,
    IOptions<IdempotencyOptions> options,
    ILogger<IdempotencyCleanupService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var settings = options.Value;

        using var timer = new PeriodicTimer(settings.CleanupInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupAsync(settings, stoppingToken);

                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "An error occurred while cleaning expired idempotency records.");

                try
                {
                    await timer.WaitForNextTickAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }

    private async Task CleanupAsync(
        IdempotencyOptions settings,
        CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IIdempotencyService>();

        var totalDeleted = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var deleted = await service.DeleteExpiredAsync(
                DateTimeOffset.UtcNow,
                settings.CleanupBatchSize,
                cancellationToken);

            totalDeleted += deleted;

            if (deleted < settings.CleanupBatchSize)
                break;
        }

        if (totalDeleted > 0)
        {
            logger.LogInformation(
                "Deleted {Count} expired idempotency records.",
                totalDeleted);
        }
    }
}
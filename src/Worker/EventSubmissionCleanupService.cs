// <copyright file="EventSubmissionCleanupService.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Worker;

using Defra.Lis.EventLogging.Repositories.Submissions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class EventSubmissionCleanupService(
    IServiceScopeFactory scopeFactory,
    IOptions<QueueOptions> options,
    ILogger<EventSubmissionCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await DeleteBatchAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(options.Value.CleanupIntervalSeconds), stoppingToken);
        }
    }

#pragma warning disable SA1202
    internal async Task<int> DeleteBatchAsync(CancellationToken cancellationToken)
#pragma warning restore SA1202
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEventSubmissionProcessingRepository>();
        var olderThan = DateTimeOffset.UtcNow.AddHours(-options.Value.SubmissionRetentionHours);
        var deleted = await repository.DeleteTerminalSubmissionsAsync(
            olderThan,
            options.Value.CleanupBatchSize,
            cancellationToken);
        if (deleted > 0)
        {
            logger.LogInformation("Deleted {SubmissionCount} expired event submissions.", deleted);
        }

        return deleted;
    }
}

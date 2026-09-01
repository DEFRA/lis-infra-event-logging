// <copyright file="OutboxPublisherService.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Worker;

using Amazon.SQS;
using Amazon.SQS.Model;
using Defra.Lis.EventLogging.Repositories.Submissions;
using Microsoft.Extensions.Options;

public class OutboxPublisherService(
    IServiceScopeFactory scopeFactory,
    IAmazonSQS sqs,
    IOptions<QueueOptions> options,
    ILogger<OutboxPublisherService> logger) : BackgroundService
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        EnsureConfigured();

        while (!stoppingToken.IsCancellationRequested)
        {
            var publishedAny = await PublishBatchAsync(stoppingToken);
            if (!publishedAny)
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(options.Value.OutboxPollIntervalSeconds),
                    stoppingToken);
            }
        }
    }

#pragma warning disable SA1202
    internal async Task<bool> PublishBatchAsync(CancellationToken cancellationToken)
#pragma warning restore SA1202
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEventSubmissionProcessingRepository>();
        var messages = await repository.GetUnpublishedAsync(options.Value.OutboxBatchSize, cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await sqs.SendMessageAsync(
                    new SendMessageRequest()
                    {
                        QueueUrl = options.Value.QueueUrl,
                        MessageBody = message.Payload.RootElement.GetRawText(),
                    },
                    cancellationToken);
                await repository.MarkPublishedAsync(message.Id, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to publish outbox message {MessageId}.", message.Id);
                await repository.MarkPublishFailedAsync(
                    message.Id,
                    exception.GetType().Name,
                    CancellationToken.None);
            }
        }

        return messages.Count > 0;
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(options.Value.QueueUrl))
        {
            throw new InvalidOperationException("EventSubmissionQueue:QueueUrl configuration is required.");
        }
    }
}

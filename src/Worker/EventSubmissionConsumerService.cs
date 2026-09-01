// <copyright file="EventSubmissionConsumerService.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Worker;

using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Defra.Lis.EventLogging.Models.Messages;
using Defra.Lis.EventLogging.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class EventSubmissionConsumerService(
    IServiceScopeFactory scopeFactory,
    IAmazonSQS sqs,
    IOptions<QueueOptions> options,
    ILogger<EventSubmissionConsumerService> logger) : BackgroundService
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        EnsureConfigured();

        while (!stoppingToken.IsCancellationRequested)
        {
            var response = await sqs.ReceiveMessageAsync(
                new ReceiveMessageRequest()
                {
                    QueueUrl = options.Value.QueueUrl,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = options.Value.WaitTimeSeconds,
                    VisibilityTimeout = options.Value.VisibilityTimeoutSeconds,
                },
                stoppingToken);

            foreach (var message in response.Messages)
            {
                await ProcessAsync(message, stoppingToken);
            }
        }
    }

#pragma warning disable SA1202
    internal async Task ProcessAsync(Message queueMessage, CancellationToken cancellationToken)
#pragma warning restore SA1202
    {
        try
        {
            var message = JsonSerializer.Deserialize<EventSubmissionMessage>(queueMessage.Body) ??
                throw new InvalidDataException("The submission message body is empty.");
            using var scope = scopeFactory.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IEventSubmissionProcessor>();
            await processor.ProcessAsync(message, cancellationToken);
            await sqs.DeleteMessageAsync(options.Value.QueueUrl, queueMessage.ReceiptHandle, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to process SQS message {MessageId}; it will become visible for retry.",
                queueMessage.MessageId);
        }
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

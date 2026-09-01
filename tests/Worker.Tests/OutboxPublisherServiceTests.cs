// <copyright file="OutboxPublisherServiceTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Worker.Tests;

using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Defra.Lis.EventLogging.Database.Entities;
using Defra.Lis.EventLogging.Repositories.Submissions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

public class OutboxPublisherServiceTests
{
    private readonly IEventSubmissionProcessingRepository repository =
        Substitute.For<IEventSubmissionProcessingRepository>();

    private readonly IAmazonSQS sqs = Substitute.For<IAmazonSQS>();

    [Fact]
    public async Task PublishBatchAsync_Should_Return_False_When_The_Outbox_Is_Empty()
    {
        repository.GetUnpublishedAsync(10, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<OutboxMessage>());
        var service = CreateService();

        var result = await service.PublishBatchAsync(TestContext.Current.CancellationToken);

        result.ShouldBeFalse();
        await sqs.DidNotReceive().SendMessageAsync(
            Arg.Any<SendMessageRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishBatchAsync_Should_Publish_And_Mark_The_Message()
    {
        var message = CreateMessage();
        repository.GetUnpublishedAsync(10, Arg.Any<CancellationToken>()).Returns([message]);
        sqs.SendMessageAsync(Arg.Any<SendMessageRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SendMessageResponse());
        var service = CreateService();

        var result = await service.PublishBatchAsync(TestContext.Current.CancellationToken);

        result.ShouldBeTrue();
        await sqs.Received().SendMessageAsync(
            Arg.Is<SendMessageRequest>(x => x != null &&
                x.QueueUrl == "queue-url" &&
                x.MessageBody == message.Payload.RootElement.GetRawText()),
            TestContext.Current.CancellationToken);
        await repository.Received().MarkPublishedAsync(
            message.Id,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task PublishBatchAsync_Should_Record_A_Publish_Failure()
    {
        var message = CreateMessage();
        repository.GetUnpublishedAsync(10, Arg.Any<CancellationToken>()).Returns([message]);
        sqs.SendMessageAsync(Arg.Any<SendMessageRequest>(), Arg.Any<CancellationToken>())
            .Returns<SendMessageResponse>(_ => throw new AmazonSQSException("unavailable"));
        var service = CreateService();

        var result = await service.PublishBatchAsync(TestContext.Current.CancellationToken);

        result.ShouldBeTrue();
        await repository.Received().MarkPublishFailedAsync(
            message.Id,
            nameof(AmazonSQSException),
            CancellationToken.None);
        await repository.DidNotReceive().MarkPublishedAsync(
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
    }

    private static OutboxMessage CreateMessage()
    {
        return new OutboxMessage()
        {
            Id = Guid.NewGuid(),
            SubmissionId = Guid.NewGuid(),
            MessageType = "CreateEvent",
            SchemaVersion = 1,
            Payload = JsonSerializer.SerializeToDocument(new { MessageType = "CreateEvent", }),
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    private OutboxPublisherService CreateService()
    {
        var services = new ServiceCollection();
        services.AddSingleton(repository);
        var provider = services.BuildServiceProvider();
        return new OutboxPublisherService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            sqs,
            Options.Create(new QueueOptions() { QueueUrl = "queue-url", }),
            Substitute.For<ILogger<OutboxPublisherService>>());
    }
}

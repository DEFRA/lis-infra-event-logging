// <copyright file="EventSubmissionConsumerServiceTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Worker.Tests;

using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Defra.Lis.EventLogging.Models.Messages;
using Defra.Lis.EventLogging.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

public class EventSubmissionConsumerServiceTests
{
    private readonly IEventSubmissionProcessor processor = Substitute.For<IEventSubmissionProcessor>();

    private readonly IAmazonSQS sqs = Substitute.For<IAmazonSQS>();

    [Fact]
    public async Task ProcessAsync_Should_Process_And_Delete_A_Valid_Message()
    {
        var submission = CreateSubmission();
        var queueMessage = CreateQueueMessage(JsonSerializer.Serialize(submission));
        var service = this.CreateService();

        await service.ProcessAsync(queueMessage, TestContext.Current.CancellationToken);

        await this.processor.Received().ProcessAsync(
            Arg.Is<EventSubmissionMessage>(x => x != null && x.SubmissionId == submission.SubmissionId),
            TestContext.Current.CancellationToken);
        await this.sqs.Received().DeleteMessageAsync(
            "queue-url",
            queueMessage.ReceiptHandle,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ProcessAsync_Should_Leave_Invalid_Messages_For_Retry()
    {
        var service = this.CreateService();

        await service.ProcessAsync(CreateQueueMessage("not-json"), TestContext.Current.CancellationToken);

        await this.processor.DidNotReceive().ProcessAsync(
            Arg.Any<EventSubmissionMessage>(),
            Arg.Any<CancellationToken>());
        await this.sqs.DidNotReceive().DeleteMessageAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_Should_Not_Delete_When_Processing_Fails()
    {
        var submission = CreateSubmission();
        this.processor.ProcessAsync(Arg.Any<EventSubmissionMessage>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new InvalidOperationException("failed"));
        var service = this.CreateService();

        await service.ProcessAsync(
            CreateQueueMessage(JsonSerializer.Serialize(submission)),
            TestContext.Current.CancellationToken);

        await this.sqs.DidNotReceive().DeleteMessageAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    private static EventSubmissionMessage CreateSubmission()
    {
        return new EventSubmissionMessage()
        {
            MessageType = "CreateEvent",
            SubmissionId = Guid.NewGuid(),
            LogId = Guid.NewGuid(),
            ShortId = "EVT-123",
        };
    }

    private static Message CreateQueueMessage(string body)
    {
        return new Message() { Body = body, MessageId = "message-id", ReceiptHandle = "receipt", };
    }

    private EventSubmissionConsumerService CreateService()
    {
        var services = new ServiceCollection();
        services.AddSingleton(this.processor);
        var provider = services.BuildServiceProvider();
        return new EventSubmissionConsumerService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            this.sqs,
            Options.Create(new QueueOptions() { QueueUrl = "queue-url", }),
            Substitute.For<ILogger<EventSubmissionConsumerService>>());
    }
}

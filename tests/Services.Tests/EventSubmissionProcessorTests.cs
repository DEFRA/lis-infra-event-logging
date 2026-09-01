// <copyright file="EventSubmissionProcessorTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services.Tests;

using Defra.Lis.EventLogging.Models.Messages;
using Defra.Lis.EventLogging.Repositories.Submissions;
using NSubstitute;

public class EventSubmissionProcessorTests
{
    private readonly IEventSubmissionProcessingRepository repository =
        Substitute.For<IEventSubmissionProcessingRepository>();

    private readonly IArtefactThumbnailProcessor thumbnailProcessor = Substitute.For<IArtefactThumbnailProcessor>();

    [Fact]
    public async Task ProcessAsync_Should_Persist_Then_Generate_The_Thumbnail()
    {
        var message = CreateMessage(Guid.NewGuid());
        var processor = new EventSubmissionProcessor(repository, thumbnailProcessor);

        await processor.ProcessAsync(message, TestContext.Current.CancellationToken);

        await repository.Received().CompleteAsync(message, TestContext.Current.CancellationToken);
        await thumbnailProcessor.Received().ProcessAsync(
            message.ArtefactId!.Value,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ProcessAsync_Should_Not_Generate_A_Thumbnail_When_There_Is_No_Artefact()
    {
        var message = CreateMessage(null);
        var processor = new EventSubmissionProcessor(repository, thumbnailProcessor);

        await processor.ProcessAsync(message, TestContext.Current.CancellationToken);

        await thumbnailProcessor.DidNotReceive().ProcessAsync(
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_Should_Record_Persistence_Failure_And_Rethrow()
    {
        var message = CreateMessage(null);
        repository.CompleteAsync(message, Arg.Any<CancellationToken>())
            .Returns<bool>(_ => throw new InvalidOperationException("database"));
        var processor = new EventSubmissionProcessor(repository, thumbnailProcessor);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            processor.ProcessAsync(message, TestContext.Current.CancellationToken));
        await repository.Received().MarkSubmissionFailedAsync(
            message.SubmissionId,
            "persistence_failed",
            CancellationToken.None);
    }

    [Fact]
    public async Task ProcessAsync_Should_Reject_Unknown_Schema_Versions()
    {
        var message = CreateMessage(null) with { SchemaVersion = 2, };
        var processor = new EventSubmissionProcessor(repository, thumbnailProcessor);

        await Should.ThrowAsync<InvalidDataException>(() =>
            processor.ProcessAsync(message, TestContext.Current.CancellationToken));
        await repository.DidNotReceive().CompleteAsync(
            Arg.Any<EventSubmissionMessage>(),
            Arg.Any<CancellationToken>());
    }

    private static EventSubmissionMessage CreateMessage(Guid? artefactId)
    {
        return new EventSubmissionMessage()
        {
            MessageType = "CreateEvent",
            SubmissionId = Guid.NewGuid(),
            LogId = Guid.NewGuid(),
            ArtefactId = artefactId,
        };
    }
}

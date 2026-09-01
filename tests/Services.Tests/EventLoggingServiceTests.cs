// <copyright file="EventLoggingServiceTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services.Tests;

using Defra.Lis.EventLogging.Database.Domain;
using Defra.Lis.EventLogging.Database.Entities;
using Defra.Lis.EventLogging.Models.Requests.Logging;
using Defra.Lis.EventLogging.Repositories.Artefacts;
using Defra.Lis.EventLogging.Repositories.Submissions;
using Defra.Lis.EventLogging.Services;
using Defra.Lis.EventLogging.Services.Models;
using NSubstitute;

public class EventLoggingServiceTests
{
    private readonly IEventSubmissionRepository repository = Substitute.For<IEventSubmissionRepository>();
    private readonly IArtefactStore store = Substitute.For<IArtefactStore>();

    [Fact]
    public async Task SubmitEventAsync_Should_Create_An_Outbox_Message_Without_An_Artefact()
    {
        var request = CreateRequest().Event;
        repository.ResolveSubTaxonomyIdAsync(
                request.Species!, request.Taxonomy!, request.SubTaxonomy!, Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());
        EventSubmission? saved = null;
        repository.CreateAsync(
                Arg.Do<EventSubmission>(x => saved = x),
                Arg.Any<OutboxMessage>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var service = new EventLoggingService(repository, store);

        var result = await service.SubmitEventAsync(
            request,
            CreateContext(),
            TestContext.Current.CancellationToken);

        result.ArtefactId.ShouldBeNull();
        saved.ShouldNotBeNull();
        saved.Type.ShouldBe(SubmissionType.CreateEvent);
        await store.DidNotReceive().PutAsync(
            Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SubmitEventAsync_Should_Reject_Unknown_Taxonomy()
    {
        var service = new EventLoggingService(repository, store);

        await Should.ThrowAsync<ArgumentException>(() => service.SubmitEventAsync(
            CreateRequest().Event,
            CreateContext(),
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SubmitEventAsync_Should_Reject_An_Idempotency_Key_Used_For_Another_Request()
    {
        var context = CreateContext();
        repository.GetByIdempotencyKeyAsync(
                context.ClientId, context.IdempotencyKey, Arg.Any<CancellationToken>())
            .Returns(new EventSubmission()
            {
                Id = Guid.NewGuid(),
                LogId = Guid.NewGuid(),
                ClientId = context.ClientId,
                IdempotencyKey = context.IdempotencyKey,
                RequestFingerprint = "different",
            });
        var service = new EventLoggingService(repository, store);

        await Should.ThrowAsync<ArgumentException>(() => service.SubmitEventAsync(
            CreateRequest().Event,
            context,
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SubmitArtefactAsync_Should_Reject_An_Unknown_Event()
    {
        repository.EventExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var service = new EventLoggingService(repository, store);

        await Should.ThrowAsync<ArgumentException>(() => service.SubmitArtefactAsync(
            Guid.NewGuid(),
            PostArtefactValidatorTestsHelper.CreateArtefact(),
            CreateContext(),
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SubmitEventWithArtefactAsync_Should_Upload_To_Event_And_Artefact_Uuid_Key()
    {
        var request = CreateRequest();
        var context = CreateContext();
        EventSubmission? savedSubmission = null;
        OutboxMessage? savedOutbox = null;
        repository.ResolveSubTaxonomyIdAsync(
                "CTT", "BIRTH", "DEFAULT", Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());
        repository.CreateAsync(
                Arg.Do<EventSubmission>(x => savedSubmission = x),
                Arg.Do<OutboxMessage>(x => savedOutbox = x),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var service = new EventLoggingService(repository, store);

        var result = await service.SubmitEventWithArtefactAsync(
            request,
            context,
            TestContext.Current.CancellationToken);

        var expectedKey = $"{result.LogId:D}/{result.ArtefactId:D}";
        await store.Received(1).PutAsync(
            expectedKey,
            request.Artefact.Content,
            "application/pdf",
            TestContext.Current.CancellationToken);
        savedSubmission.ShouldNotBeNull();
        savedSubmission.PendingS3Key.ShouldBe(expectedKey);
        savedSubmission.OriginalFilename.ShouldBe("original-report.pdf");
        savedSubmission.MimeType.ShouldBe("application/pdf");
        savedOutbox.ShouldNotBeNull();
        savedOutbox.Payload.RootElement.GetProperty("PendingS3Key").GetString().ShouldBe(expectedKey);
        savedOutbox.Payload.RootElement.GetProperty("OriginalFilename").GetString()
            .ShouldBe("original-report.pdf");
    }

    [Fact]
    public async Task SubmitArtefactAsync_Should_Use_The_Existing_Event_Folder()
    {
        var logId = Guid.NewGuid();
        var request = PostArtefactValidatorTestsHelper.CreateArtefact();
        repository.EventExistsAsync(logId, Arg.Any<CancellationToken>()).Returns(true);
        EventSubmission? saved = null;
        repository.CreateAsync(
                Arg.Do<EventSubmission>(x => saved = x),
                Arg.Any<OutboxMessage>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var service = new EventLoggingService(repository, store);

        var result = await service.SubmitArtefactAsync(
            logId,
            request,
            CreateContext(),
            TestContext.Current.CancellationToken);

        result.LogId.ShouldBe(logId);
        saved.ShouldNotBeNull();
        await store.Received(1).PutAsync(
            $"{logId:D}/{result.ArtefactId:D}",
            request.Content,
            request.MimeType,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task SubmitEventWithArtefactAsync_Should_Delete_Staged_Object_When_Persistence_Fails()
    {
        var request = CreateRequest();
        repository.ResolveSubTaxonomyIdAsync(
                "CTT", "BIRTH", "DEFAULT", Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());
        EventSubmission? attemptedSubmission = null;
        repository.CreateAsync(
                Arg.Do<EventSubmission>(x => attemptedSubmission = x),
                Arg.Any<OutboxMessage>(),
                Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("Database unavailable"));
        var service = new EventLoggingService(repository, store);

        var action = () => service.SubmitEventWithArtefactAsync(
            request,
            CreateContext(),
            TestContext.Current.CancellationToken);

        await action.ShouldThrowAsync<InvalidOperationException>();
        attemptedSubmission.ShouldNotBeNull();
        await store.Received(1).DeleteAsync(
            $"{attemptedSubmission.LogId:D}/{attemptedSubmission.ArtefactId:D}",
            CancellationToken.None);
    }

    [Fact]
    public async Task SubmitEventWithArtefactAsync_Should_Not_Upload_Again_For_Idempotent_Retry()
    {
        var request = CreateRequest();
        var context = CreateContext();
        var firstService = new EventLoggingService(repository, store);
        repository.ResolveSubTaxonomyIdAsync(
                "CTT", "BIRTH", "DEFAULT", Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());
        EventSubmission? saved = null;
        repository.CreateAsync(
                Arg.Do<EventSubmission>(x => saved = x),
                Arg.Any<OutboxMessage>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        await firstService.SubmitEventWithArtefactAsync(
            request,
            context,
            TestContext.Current.CancellationToken);
        repository.GetByIdempotencyKeyAsync(
                context.ClientId,
                context.IdempotencyKey,
                Arg.Any<CancellationToken>())
            .Returns(saved);

        await firstService.SubmitEventWithArtefactAsync(
            request,
            context,
            TestContext.Current.CancellationToken);

        await store.Received(1).PutAsync(
            Arg.Any<string>(),
            Arg.Any<Stream>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    private static PostEventWithArtefact CreateRequest()
    {
        return new PostEventWithArtefact()
        {
            Event = new PostEvent()
            {
                CountyParishHolding = "12/345/6789",
                Title = "Birth event",
                CreatedBy = "test-client",
                Species = "CTT",
                Taxonomy = "BIRTH",
                SubTaxonomy = "DEFAULT",
            },
            Artefact = PostArtefactValidatorTestsHelper.CreateArtefact(),
        };
    }

    private static SubmissionContext CreateContext()
    {
        return new SubmissionContext()
        {
            ClientId = "client",
            IdempotencyKey = Guid.NewGuid().ToString(),
            CorrelationId = Guid.NewGuid(),
        };
    }

    private static class PostArtefactValidatorTestsHelper
    {
        public static PostArtefact CreateArtefact()
        {
            return new PostArtefact()
            {
                Content = new MemoryStream([1, 2, 3]),
                MimeType = "application/pdf",
                OriginalFilename = "original-report.pdf",
                Size = 3,
            };
        }
    }
}

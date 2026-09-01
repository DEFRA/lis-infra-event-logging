// <copyright file="ArtefactThumbnailProcessorTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services.Tests;

using Defra.Lis.EventLogging.Database.Entities;
using Defra.Lis.EventLogging.Repositories.Artefacts;
using Defra.Lis.EventLogging.Services.Thumbnails;
using NSubstitute;

public class ArtefactThumbnailProcessorTests
{
    private readonly IArtefactRepository repository = Substitute.For<IArtefactRepository>();
    private readonly IArtefactStore store = Substitute.For<IArtefactStore>();
    private readonly IThumbnailService thumbnailService = Substitute.For<IThumbnailService>();

    [Theory]
    [InlineData(ThumbnailStatus.Available)]
    [InlineData(ThumbnailStatus.Unsupported)]
    public async Task ProcessAsync_Should_Be_Idempotent_For_Terminal_Statuses(ThumbnailStatus status)
    {
        var reference = CreateReference(status);
        repository.GetForThumbnailAsync(reference.Id, Arg.Any<CancellationToken>()).Returns(reference);
        var processor = new ArtefactThumbnailProcessor(repository, store, thumbnailService);

        await processor.ProcessAsync(reference.Id, TestContext.Current.CancellationToken);

        await store.DidNotReceive().GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_Should_Record_Unsupported_Media_Without_Failing()
    {
        var reference = CreateReference(ThumbnailStatus.Pending, "text/plain");
        repository.GetForThumbnailAsync(reference.Id, Arg.Any<CancellationToken>()).Returns(reference);
        thumbnailService.Supports(reference.MimeType).Returns(false);
        var processor = new ArtefactThumbnailProcessor(repository, store, thumbnailService);

        await processor.ProcessAsync(reference.Id, TestContext.Current.CancellationToken);

        await repository.Received().SetThumbnailStatusAsync(
            reference.Id,
            ThumbnailStatus.Unsupported,
            "unsupported_media_type",
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ProcessAsync_Should_Save_A_Generated_Thumbnail()
    {
        var reference = CreateReference(ThumbnailStatus.Pending);
        var content = new MemoryStream([1, 2, 3]);
        var generated = new GeneratedThumbnail()
        {
            Content = [4, 5, 6],
            MimeType = "image/webp",
            Width = 320,
            Height = 180,
        };
        repository.GetForThumbnailAsync(reference.Id, Arg.Any<CancellationToken>()).Returns(reference);
        thumbnailService.Supports(reference.MimeType).Returns(true);
        store.GetAsync(reference.S3Path, Arg.Any<CancellationToken>())
            .Returns(new StoredArtefact() { Content = content, ContentLength = 3, });
        thumbnailService.GenerateAsync(reference.MimeType, content, Arg.Any<CancellationToken>())
            .Returns(generated);
        var processor = new ArtefactThumbnailProcessor(repository, store, thumbnailService);

        await processor.ProcessAsync(reference.Id, TestContext.Current.CancellationToken);

        await repository.Received().SaveThumbnailAsync(
            reference.Id,
            Arg.Is<ThumbnailPersistence>(x => x != null &&
                x.Content.SequenceEqual(generated.Content) &&
                x.MimeType == generated.MimeType &&
                x.Width == generated.Width &&
                x.Height == generated.Height),
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ProcessAsync_Should_Record_And_Rethrow_Generation_Failures()
    {
        var reference = CreateReference(ThumbnailStatus.Pending);
        var content = new MemoryStream([1, 2, 3]);
        repository.GetForThumbnailAsync(reference.Id, Arg.Any<CancellationToken>()).Returns(reference);
        thumbnailService.Supports(reference.MimeType).Returns(true);
        store.GetAsync(reference.S3Path, Arg.Any<CancellationToken>())
            .Returns(new StoredArtefact() { Content = content, ContentLength = 3, });
        thumbnailService.GenerateAsync(reference.MimeType, content, Arg.Any<CancellationToken>())
            .Returns<GeneratedThumbnail>(_ => throw new InvalidDataException("invalid"));
        var processor = new ArtefactThumbnailProcessor(repository, store, thumbnailService);

        await Should.ThrowAsync<InvalidDataException>(() =>
            processor.ProcessAsync(reference.Id, TestContext.Current.CancellationToken));
        await repository.Received().SetThumbnailStatusAsync(
            reference.Id,
            ThumbnailStatus.Failed,
            "generation_failed",
            CancellationToken.None);
    }

    private static ArtefactThumbnailReference CreateReference(
        ThumbnailStatus status,
        string mimeType = "image/png")
    {
        return new ArtefactThumbnailReference()
        {
            Id = Guid.NewGuid(),
            S3Path = $"events/{Guid.NewGuid()}",
            MimeType = mimeType,
            Status = status,
        };
    }
}

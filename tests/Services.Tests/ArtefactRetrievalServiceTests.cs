// <copyright file="ArtefactRetrievalServiceTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services.Tests;

using Defra.Lis.EventLogging.Repositories.Artefacts;
using Defra.Lis.EventLogging.Services;
using NSubstitute;

public class ArtefactRetrievalServiceTests
{
    private readonly IArtefactRepository repository = Substitute.For<IArtefactRepository>();
    private readonly IArtefactStore store = Substitute.For<IArtefactStore>();

    [Fact]
    public async Task GetArtefactAsync_Should_Return_Null_When_Artefact_Does_Not_Belong_To_Event()
    {
        this.repository.GetForEventAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns((ArtefactStorageReference?)null);
        var service = new ArtefactRetrievalService(this.repository, this.store);

        var result = await service.GetArtefactAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TestContext.Current.CancellationToken);

        result.ShouldBeNull();
        await this.store.DidNotReceive().GetAsync(
            Arg.Any<string>(),
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetArtefactAsync_Should_Return_Null_When_Object_Is_Missing_From_Storage()
    {
        var reference = CreateReference();
        this.repository.GetForEventAsync(
                Arg.Any<Guid>(),
                reference.Id,
                Arg.Any<CancellationToken>())
            .Returns(reference);
        this.store.GetAsync(reference.S3Path, Arg.Any<CancellationToken>())
            .Returns((StoredArtefact?)null);
        var service = new ArtefactRetrievalService(this.repository, this.store);

        var result = await service.GetArtefactAsync(
            Guid.NewGuid(),
            reference.Id,
            TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetArtefactAsync_Should_Map_Database_Metadata_And_Stored_Content()
    {
        var reference = CreateReference();
        var content = new MemoryStream([1, 2, 3]);
        this.repository.GetForEventAsync(
                Arg.Any<Guid>(),
                reference.Id,
                Arg.Any<CancellationToken>())
            .Returns(reference);
        this.store.GetAsync(reference.S3Path, Arg.Any<CancellationToken>())
            .Returns(new StoredArtefact() { Content = content, ContentLength = 3, });
        var service = new ArtefactRetrievalService(this.repository, this.store);

        var result = await service.GetArtefactAsync(
            Guid.NewGuid(),
            reference.Id,
            TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.Content.ShouldBeSameAs(content);
        result.ContentLength.ShouldBe(3);
        result.MimeType.ShouldBe("application/pdf");
        result.Filename.ShouldBe("evidence.pdf");
    }

    private static ArtefactStorageReference CreateReference()
    {
        return new ArtefactStorageReference()
        {
            Id = Guid.NewGuid(),
            S3Path = "events/evidence.pdf",
            MimeType = "application/pdf",
            OriginalFilename = "evidence.pdf",
        };
    }
}

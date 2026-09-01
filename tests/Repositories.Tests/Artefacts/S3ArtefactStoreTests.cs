// <copyright file="S3ArtefactStoreTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Repositories.Tests.Artefacts;

using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Defra.Lis.EventLogging.Repositories.Artefacts;
using Microsoft.Extensions.Options;
using NSubstitute;

public class S3ArtefactStoreTests
{
    private readonly IAmazonS3 s3Client = Substitute.For<IAmazonS3>();

    [Fact]
    public async Task PutAsync_Should_Preserve_The_Provided_Uuid_Key_And_Content_Type()
    {
        var content = new MemoryStream([1, 2, 3]);
        var eventId = Guid.NewGuid();
        var artefactId = Guid.NewGuid();
        var key = $"{eventId:D}/{artefactId:D}";
        var store = CreateStore(s3Client);

        await store.PutAsync(key, content, "application/pdf", TestContext.Current.CancellationToken);

        await s3Client.Received(1).PutObjectAsync(
            Arg.Is<PutObjectRequest>(x =>
                x != null &&
                x.Key == key &&
                x.InputStream == content &&
                x.ContentType == "application/pdf" &&
                !x.AutoCloseStream),
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_The_Staged_Key()
    {
        var store = CreateStore(s3Client);

        await store.DeleteAsync("event-id/artefact-id", TestContext.Current.CancellationToken);

        await s3Client.Received(1).DeleteObjectAsync(
            "event-logging-artefacts",
            "event-id/artefact-id",
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetAsync_Should_Return_The_S3_Stream()
    {
        var content = new MemoryStream([1, 2, 3]);
        s3Client.GetObjectAsync(Arg.Any<GetObjectRequest>(), Arg.Any<CancellationToken>())
            .Returns(new GetObjectResponse() { ResponseStream = content, ContentLength = 3, });
        var store = CreateStore(s3Client);

        var result = await store.GetAsync("events/item.pdf", TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.Content.ShouldBeSameAs(content);
        result.ContentLength.ShouldBe(3);
        await s3Client.Received(1).GetObjectAsync(
            Arg.Is<GetObjectRequest>(x =>
                x != null &&
                x.BucketName == "event-logging-artefacts" &&
                x.Key == "events/item.pdf"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAsync_Should_Return_Null_When_S3_Object_Does_Not_Exist()
    {
        s3Client.GetObjectAsync(Arg.Any<GetObjectRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task<GetObjectResponse>>(_ => throw new AmazonS3Exception("Not found")
            {
                StatusCode = HttpStatusCode.NotFound,
            });
        var store = CreateStore(s3Client);

        var result = await store.GetAsync("missing", TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAsync_Should_Reject_Missing_Bucket_Configuration()
    {
        var store = new S3ArtefactStore(
            s3Client,
            Options.Create(new ArtefactStorageOptions()));

        var action = () => store.GetAsync("item", TestContext.Current.CancellationToken);

        await action.ShouldThrowAsync<InvalidOperationException>();
    }

    private static S3ArtefactStore CreateStore(IAmazonS3 s3Client)
    {
        return new S3ArtefactStore(
            s3Client,
            Options.Create(new ArtefactStorageOptions() { BucketName = "event-logging-artefacts", }));
    }
}

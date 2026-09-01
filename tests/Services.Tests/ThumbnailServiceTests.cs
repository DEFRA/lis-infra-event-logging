// <copyright file="ThumbnailServiceTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services.Tests;

using Defra.Lis.EventLogging.Services.Thumbnails;
using NSubstitute;

public class ThumbnailServiceTests
{
    [Fact]
    public void Supports_Should_Return_True_When_A_Generator_Matches()
    {
        var generator = Substitute.For<IThumbnailGenerator>();
        generator.Supports("image/png").Returns(true);
        var service = new ThumbnailService([generator]);

        service.Supports("image/png").ShouldBeTrue();
    }

    [Fact]
    public async Task GenerateAsync_Should_Delegate_To_The_Matching_Generator()
    {
        var generated = new GeneratedThumbnail()
        {
            Content = [1], MimeType = "image/webp", Width = 1, Height = 1,
        };
        var generator = Substitute.For<IThumbnailGenerator>();
        generator.Supports("image/png").Returns(true);
        generator.GenerateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(generated);
        var service = new ThumbnailService([generator]);
        using var content = new MemoryStream([1]);

        var result = await service.GenerateAsync(
            "image/png",
            content,
            TestContext.Current.CancellationToken);

        result.ShouldBeSameAs(generated);
    }

    [Fact]
    public async Task GenerateAsync_Should_Reject_Unsupported_Media()
    {
        var service = new ThumbnailService([]);
        using var content = new MemoryStream();

        await Should.ThrowAsync<NotSupportedException>(() =>
            service.GenerateAsync("text/plain", content, TestContext.Current.CancellationToken));
    }
}

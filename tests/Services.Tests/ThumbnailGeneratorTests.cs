// <copyright file="ThumbnailGeneratorTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services.Tests;

using Defra.Lis.EventLogging.Services.Thumbnails;
using SkiaSharp;

public class ThumbnailGeneratorTests
{
    [Theory]
    [InlineData("image/jpeg", true)]
    [InlineData("image/png", true)]
    [InlineData("image/webp", true)]
    [InlineData("IMAGE/PNG", true)]
    [InlineData("image/gif", false)]
    public void Image_Generator_Should_Recognise_Supported_Mime_Types(string mimeType, bool expected)
    {
        new ImageThumbnailGenerator().Supports(mimeType).ShouldBe(expected);
    }

    [Theory]
    [InlineData("application/pdf", true)]
    [InlineData("APPLICATION/PDF", true)]
    [InlineData("text/plain", false)]
    public void Pdf_Generator_Should_Recognise_Supported_Mime_Types(string mimeType, bool expected)
    {
        new PdfThumbnailGenerator().Supports(mimeType).ShouldBe(expected);
    }

    [Fact]
    public async Task Image_Generator_Should_Reject_Invalid_Content()
    {
        using var content = new MemoryStream([1, 2, 3]);

        await Should.ThrowAsync<InvalidDataException>(() =>
            new ImageThumbnailGenerator().GenerateAsync(content, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Image_Generator_Should_Honour_Cancellation()
    {
        using var content = CreatePng(10, 10);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(() =>
            new ImageThumbnailGenerator().GenerateAsync(content, cancellation.Token));
    }

    [Fact]
    public async Task Image_Generator_Should_Fit_Landscape_Image_Within_The_Bounds()
    {
        using var content = CreatePng(640, 360);
        var generator = new ImageThumbnailGenerator();

        var result = await generator.GenerateAsync(content, TestContext.Current.CancellationToken);

        result.MimeType.ShouldBe("image/webp");
        result.Width.ShouldBe(320);
        result.Height.ShouldBe(180);
        result.Content.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Image_Generator_Should_Not_Upscale_Small_Images()
    {
        using var content = CreatePng(100, 50);
        var generator = new ImageThumbnailGenerator();

        var result = await generator.GenerateAsync(content, TestContext.Current.CancellationToken);

        result.Width.ShouldBe(100);
        result.Height.ShouldBe(50);
    }

    [Fact]
    public async Task Pdf_Generator_Should_Render_The_First_Page()
    {
        using var content = new MemoryStream();
        using (var document = SKDocument.CreatePdf(content))
        {
            using var canvas = document.BeginPage(640, 360);
            canvas.DrawColor(SKColors.White);
            document.EndPage();
            document.Close();
        }

        content.Position = 0;
        var generator = new PdfThumbnailGenerator();

        var result = await generator.GenerateAsync(content, TestContext.Current.CancellationToken);

        result.MimeType.ShouldBe("image/webp");
        result.Width.ShouldBe(320);
        result.Height.ShouldBe(180);
    }

    private static MemoryStream CreatePng(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.CornflowerBlue);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return new MemoryStream(data.ToArray());
    }
}

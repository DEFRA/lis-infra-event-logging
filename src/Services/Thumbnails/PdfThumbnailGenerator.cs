// <copyright file="PdfThumbnailGenerator.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services.Thumbnails;

using PDFtoImage;

public class PdfThumbnailGenerator : IThumbnailGenerator
{
    private static readonly SemaphoreSlim RenderLock = new(1, 1);

    public bool Supports(string mimeType)
    {
        return string.Equals(mimeType, "application/pdf", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<GeneratedThumbnail> GenerateAsync(
        Stream content,
        CancellationToken cancellationToken = default)
    {
        await RenderLock.WaitAsync(cancellationToken);

        try
        {
#pragma warning disable CA1416
            using var bitmap = Conversion.ToImage(content, page: 0);
#pragma warning restore CA1416
            return ThumbnailEncoder.Encode(bitmap);
        }
        finally
        {
            RenderLock.Release();
        }
    }
}

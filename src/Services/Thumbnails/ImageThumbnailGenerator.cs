// <copyright file="ImageThumbnailGenerator.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services.Thumbnails;

using SkiaSharp;

public class ImageThumbnailGenerator : IThumbnailGenerator
{
    private static readonly HashSet<string> SupportedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
    };

    public bool Supports(string mimeType)
    {
        return SupportedMimeTypes.Contains(mimeType);
    }

    public Task<GeneratedThumbnail> GenerateAsync(
        Stream content,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var codec = SKCodec.Create(content) ?? throw new InvalidDataException("Invalid image content.");
        if ((long)codec.Info.Width * codec.Info.Height > ThumbnailEncoder.MaximumPixels)
        {
            throw new InvalidDataException("The source image exceeds the thumbnail pixel limit.");
        }

        using var bitmap = SKBitmap.Decode(codec) ?? throw new InvalidDataException("Invalid image content.");
        return Task.FromResult(ThumbnailEncoder.Encode(bitmap));
    }
}

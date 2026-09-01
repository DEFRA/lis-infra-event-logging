// <copyright file="ThumbnailEncoder.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services.Thumbnails;

using SkiaSharp;

internal static class ThumbnailEncoder
{
    internal const int MaximumDimension = 320;
    internal const long MaximumPixels = 40_000_000;
    internal const int MaximumOutputBytes = 1_000_000;

    public static GeneratedThumbnail Encode(SKBitmap source)
    {
        if ((long)source.Width * source.Height > MaximumPixels)
        {
            throw new InvalidDataException("The source image exceeds the thumbnail pixel limit.");
        }

        var scale = Math.Min(
            1D,
            Math.Min((double)MaximumDimension / source.Width, (double)MaximumDimension / source.Height));
        var width = Math.Max(1, (int)Math.Round(source.Width * scale));
        var height = Math.Max(1, (int)Math.Round(source.Height * scale));

        using var resized = source.Resize(
            new SKImageInfo(width, height),
            new SKSamplingOptions(SKCubicResampler.Mitchell));
        using var image = SKImage.FromBitmap(resized ?? throw new InvalidDataException("Unable to resize image."));
        using var data = image.Encode(SKEncodedImageFormat.Webp, 80) ??
            throw new InvalidDataException("Unable to encode thumbnail.");
        var content = data.ToArray();

        if (content.Length > MaximumOutputBytes)
        {
            throw new InvalidDataException("The generated thumbnail exceeds the size limit.");
        }

        return new GeneratedThumbnail()
        {
            Content = content,
            MimeType = "image/webp",
            Width = width,
            Height = height,
        };
    }
}

// <copyright file="ThumbnailService.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services.Thumbnails;

public class ThumbnailService(IEnumerable<IThumbnailGenerator> generators) : IThumbnailService
{
    public bool Supports(string mimeType)
    {
        return generators.Any(x => x.Supports(mimeType));
    }

    public Task<GeneratedThumbnail> GenerateAsync(
        string mimeType,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var generator = generators.FirstOrDefault(x => x.Supports(mimeType)) ??
            throw new NotSupportedException($"Thumbnail generation is not supported for '{mimeType}'.");

        return generator.GenerateAsync(content, cancellationToken);
    }
}

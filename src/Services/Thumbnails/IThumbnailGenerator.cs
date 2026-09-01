// <copyright file="IThumbnailGenerator.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services.Thumbnails;

public interface IThumbnailGenerator
{
    bool Supports(string mimeType);

    Task<GeneratedThumbnail> GenerateAsync(Stream content, CancellationToken cancellationToken = default);
}

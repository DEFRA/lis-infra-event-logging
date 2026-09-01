// <copyright file="IThumbnailService.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services.Thumbnails;

public interface IThumbnailService
{
    bool Supports(string mimeType);

    Task<GeneratedThumbnail> GenerateAsync(
        string mimeType,
        Stream content,
        CancellationToken cancellationToken = default);
}

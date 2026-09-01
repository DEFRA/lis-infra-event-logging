// <copyright file="GeneratedThumbnail.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services.Thumbnails;

public record GeneratedThumbnail
{
    public required byte[] Content { get; init; }

    public required string MimeType { get; init; }

    public required int Width { get; init; }

    public required int Height { get; init; }
}

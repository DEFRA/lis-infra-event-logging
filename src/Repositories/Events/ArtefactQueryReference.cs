// <copyright file="ArtefactQueryReference.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Repositories.Events;

using Defra.Lis.EventLogging.Database.Entities;

public record ArtefactQueryReference
{
    public required Guid Id { get; init; }

    public byte[]? Thumbnail { get; init; }

    public string? ThumbnailMimeType { get; init; }

    public int? ThumbnailWidth { get; init; }

    public int? ThumbnailHeight { get; init; }

    public ThumbnailStatus ThumbnailStatus { get; init; }
}

// <copyright file="ArtefactThumbnailReference.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Repositories.Artefacts;

using Defra.Lis.EventLogging.Database.Entities;

public record ArtefactThumbnailReference
{
    public required Guid Id { get; init; }

    public required string S3Path { get; init; }

    public required string MimeType { get; init; }

    public required ThumbnailStatus Status { get; init; }
}

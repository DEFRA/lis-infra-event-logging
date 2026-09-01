// <copyright file="ArtefactDownload.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services.Models;

public record ArtefactDownload
{
    public required Stream Content { get; init; }

    public required string MimeType { get; init; }

    public required string Filename { get; init; }

    public long? ContentLength { get; init; }
}

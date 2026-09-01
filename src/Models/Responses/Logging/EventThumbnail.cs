// <copyright file="EventThumbnail.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Responses.Logging;

public record EventThumbnail
{
    public required string MimeType { get; init; }

    public required int Width { get; init; }

    public required int Height { get; init; }

    public required byte[] Content { get; init; }
}

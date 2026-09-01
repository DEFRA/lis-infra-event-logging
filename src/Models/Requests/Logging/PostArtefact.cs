// <copyright file="PostArtefact.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Requests.Logging;

public record PostArtefact
{
    public required Stream Content { get; init; }

    public required string MimeType { get; init; }

    public required string OriginalFilename { get; init; }

    public required long Size { get; init; }
}

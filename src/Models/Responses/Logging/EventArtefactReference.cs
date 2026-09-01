// <copyright file="EventArtefactReference.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Responses.Logging;

public record EventArtefactReference
{
    public required Guid Id { get; init; }

    public EventThumbnail? Thumbnail { get; init; }
}

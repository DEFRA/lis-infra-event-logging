// <copyright file="EventQueryItem.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Repositories.Events;

using EventEntity = Defra.Lis.EventLogging.Database.Entities.Event;

public record EventQueryItem
{
    public required EventEntity Event { get; init; }

    public required IReadOnlyCollection<ArtefactQueryReference> Artefacts { get; init; }
}

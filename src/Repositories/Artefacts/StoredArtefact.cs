// <copyright file="StoredArtefact.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Repositories.Artefacts;

public record StoredArtefact
{
    public required Stream Content { get; init; }

    public long? ContentLength { get; init; }
}

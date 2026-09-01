// <copyright file="PostEventWithArtefact.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Requests.Logging;

public record PostEventWithArtefact
{
    public required PostEvent Event { get; init; }

    public required PostArtefact Artefact { get; init; }
}

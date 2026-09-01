// <copyright file="EventAcceptedResult.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Responses.Logging;

public record EventAcceptedResult
{
    public required Guid LogId { get; init; }

    public Guid? ArtefactId { get; init; }
}

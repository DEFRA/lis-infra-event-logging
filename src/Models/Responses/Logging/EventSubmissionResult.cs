// <copyright file="EventSubmissionResult.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Responses.Logging;

public record EventSubmissionResult
{
    public required Guid SubmissionId { get; init; }

    public required Guid LogId { get; init; }

    public Guid? ArtefactId { get; init; }

    public required string ShortId { get; init; }

    public required SubmissionStatus Status { get; init; }
}

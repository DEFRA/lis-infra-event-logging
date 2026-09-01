// <copyright file="EventSubmissionStatusResult.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Responses.Logging;

public record EventSubmissionStatusResult : EventSubmissionResult
{
    public required DateTimeOffset SubmittedAt { get; init; }

    public DateTimeOffset? CompletedAt { get; init; }

    public string? FailureCode { get; init; }
}

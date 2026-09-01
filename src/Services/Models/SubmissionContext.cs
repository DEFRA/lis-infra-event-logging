// <copyright file="SubmissionContext.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services.Models;

public record SubmissionContext
{
    public required string ClientId { get; init; }

    public required string IdempotencyKey { get; init; }

    public required Guid CorrelationId { get; init; }
}

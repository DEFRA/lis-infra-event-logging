// <copyright file="EventResult.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Responses.Logging;

using System.Text.Json;

public record EventResult
{
    public required Guid LogId { get; init; }

    public required string ShortId { get; init; }

    public required string CountyParishHolding { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required string Title { get; init; }

    public required Guid SubTaxonomyId { get; init; }

    public JsonDocument? Data { get; init; }

    public required string CreatedBy { get; init; }

    public IReadOnlyCollection<EventArtefactReference> Artefacts { get; init; } = [];
}

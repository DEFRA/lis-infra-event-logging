// <copyright file="PostEvent.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Requests.Logging;

using System.Text.Json;

public record PostEvent
{
    public string? CountyParishHolding { get; set; }

    public DateTimeOffset CreatedAt { get; init; } = DateTime.UtcNow;

    public string? Title { get; set; }

    public JsonDocument? Data { get; set; }

    public string? CreatedBy { get; set; }

    public string? Taxonomy { get; set; }

    public string? SubTaxonomy { get; set; }

    public string? Species { get; set; }
}

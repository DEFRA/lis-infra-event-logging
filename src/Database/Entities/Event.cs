// <copyright file="Event.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database.Entities;

using System.Text.Json;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class Event
{
    public Guid Id { get; set; }

    public string UrlShortCode { get; set; } = null!;

    public string CountyParishHolding { get; set; } = null!;

    public string County { get; private set; } = null!;

    public string Parish { get; private set; } = null!;

    public string Holding { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public string Title { get; set; } = null!;

    public Guid SubTaxonomyId { get; set; }

    public JsonDocument? Data { get; set; }

    public string CreatedBy { get; set; } = null!;

    public EventSubTaxonomy SubTaxonomy { get; set; } = null!;

    public ICollection<EventArtefact> Artefacts { get; set; } = [];

    public ICollection<EventExtractedValue> ExtractedValues { get; set; } = [];
}

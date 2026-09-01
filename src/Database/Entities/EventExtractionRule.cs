// <copyright file="EventExtractionRule.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database.Entities;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class EventExtractionRule
{
    public Guid Id { get; set; }

    public Guid SubTaxonomyId { get; set; }

    public Guid TokenId { get; set; }

    public string JsonPath { get; set; } = null!;

    public string ValueType { get; set; } = null!;

    public bool Required { get; set; }

    public bool AllowsMultiple { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public EventSubTaxonomy SubTaxonomy { get; set; } = null!;

    public EventExtractionToken Token { get; set; } = null!;

    public ICollection<EventExtractedValue> Values { get; set; } = [];
}

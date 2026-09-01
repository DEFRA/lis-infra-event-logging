// <copyright file="EventSubTaxonomy.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database.Entities;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class EventSubTaxonomy
{
    public Guid Id { get; set; }

    public Guid TaxonomyId { get; set; }

    public Guid SpeciesId { get; set; }

    public string Name { get; set; } = null!;

    public EventTaxonomy Taxonomy { get; set; } = null!;

    public EventSpecies Species { get; set; } = null!;

    public ICollection<Event> Events { get; set; } = [];

    public ICollection<EventExtractionRule> ExtractionRules { get; set; } = [];
}

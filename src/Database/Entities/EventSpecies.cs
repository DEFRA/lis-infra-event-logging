// <copyright file="EventSpecies.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database.Entities;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class EventSpecies
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public ICollection<EventSubTaxonomy> SubTaxonomies { get; set; } = [];
}

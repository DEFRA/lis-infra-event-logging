// <copyright file="EventExtractionToken.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database.Entities;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class EventExtractionToken
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public ICollection<EventExtractionRule> Rules { get; set; } = [];
}

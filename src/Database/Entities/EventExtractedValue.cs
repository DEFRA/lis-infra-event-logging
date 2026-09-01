// <copyright file="EventExtractedValue.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database.Entities;

using System.Text.Json;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class EventExtractedValue
{
    public Guid EventId { get; set; }

    public Guid ExtractionRuleId { get; set; }

    public Guid SubTaxonomyId { get; set; }

    public string ValueType { get; set; } = null!;

    public int ValueOrdinal { get; set; }

    public string? ValueText { get; set; }

    public decimal? ValueNumber { get; set; }

    public bool? ValueBoolean { get; set; }

    public DateOnly? ValueDate { get; set; }

    public DateTimeOffset? ValueTimestamp { get; set; }

    public Guid? ValueUuid { get; set; }

    public JsonDocument? ValueJson { get; set; }

    public Event Event { get; set; } = null!;

    public EventExtractionRule ExtractionRule { get; set; } = null!;
}

// <copyright file="EventExtractedValueConfiguration.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database.Configuration;

using Defra.Database.Postgres;
using Defra.Lis.EventLogging.Database.Entities;

public sealed class EventExtractedValueConfiguration : IEntityTypeConfiguration<EventExtractedValue>
{
    public void Configure(EntityTypeBuilder<EventExtractedValue> builder)
    {
        builder.ToTable("event_extracted_values");
        builder.HasKey(x => new { x.EventId, x.ExtractionRuleId, x.ValueOrdinal });
        builder.Property(x => x.ValueJson).HasColumnType(ColumnTypes.JsonB);
        builder.HasOne(x => x.Event)
            .WithMany(x => x.ExtractedValues)
            .HasForeignKey(x => new { x.EventId, x.SubTaxonomyId })
            .HasPrincipalKey(x => new { x.Id, x.SubTaxonomyId })
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ExtractionRule)
            .WithMany(x => x.Values)
            .HasForeignKey(x => new { x.ExtractionRuleId, x.SubTaxonomyId, x.ValueType })
            .HasPrincipalKey(x => new { x.Id, x.SubTaxonomyId, x.ValueType });
    }
}

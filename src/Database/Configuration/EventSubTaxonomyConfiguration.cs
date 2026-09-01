// <copyright file="EventSubTaxonomyConfiguration.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database.Configuration;

using Defra.Lis.EventLogging.Database.Entities;

public sealed class EventSubTaxonomyConfiguration : IEntityTypeConfiguration<EventSubTaxonomy>
{
    public void Configure(EntityTypeBuilder<EventSubTaxonomy> builder)
    {
        builder.ToTable("event_sub_taxonomies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
        builder.Property(x => x.Name).IsRequired();
        builder.HasIndex(x => new { x.TaxonomyId, x.SpeciesId, x.Name }).IsUnique();
        builder.HasOne(x => x.Taxonomy).WithMany(x => x.SubTaxonomies).HasForeignKey(x => x.TaxonomyId);
        builder.HasOne(x => x.Species).WithMany(x => x.SubTaxonomies).HasForeignKey(x => x.SpeciesId);
    }
}

// <copyright file="EventTaxonomyConfiguration.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database.Configuration;

using Defra.Lis.EventLogging.Database.Entities;

public sealed class EventTaxonomyConfiguration : IEntityTypeConfiguration<EventTaxonomy>
{
    public void Configure(EntityTypeBuilder<EventTaxonomy> builder)
    {
        builder.ToTable("event_taxonomies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
        builder.Property(x => x.Name).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

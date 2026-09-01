// <copyright file="EventSpeciesConfiguration.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database.Configuration;

using Defra.Lis.EventLogging.Database.Entities;

public sealed class EventSpeciesConfiguration : IEntityTypeConfiguration<EventSpecies>
{
    public void Configure(EntityTypeBuilder<EventSpecies> builder)
    {
        builder.ToTable("event_species");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
        builder.Property(x => x.Name).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

// <copyright file="EventConfiguration.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database.Configuration;

using Defra.Database.Postgres;
using Defra.Lis.EventLogging.Database.Entities;

public sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("events");
        builder.HasKey(x => x.Id);
        builder.HasAlternateKey(x => new { x.Id, x.SubTaxonomyId });
        builder.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
        builder.Property(x => x.UrlShortCode).HasMaxLength(32).IsRequired().ValueGeneratedOnAdd();
        builder.HasIndex(x => x.UrlShortCode).IsUnique();
        builder.HasIndex(x => new { x.CreatedAt, x.Id });
        builder.HasIndex(x => new { x.CountyParishHolding, x.CreatedAt, x.Id });
        builder.Property(x => x.CountyParishHolding).IsRequired();
        builder.Property(x => x.County).ValueGeneratedOnAddOrUpdate();
        builder.Property(x => x.Parish).ValueGeneratedOnAddOrUpdate();
        builder.Property(x => x.Holding).ValueGeneratedOnAddOrUpdate();
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.Title).IsRequired();
        builder.Property(x => x.Data).HasColumnType(ColumnTypes.JsonB);
        builder.Property(x => x.CreatedBy).IsRequired();
        builder.HasOne(x => x.SubTaxonomy).WithMany(x => x.Events).HasForeignKey(x => x.SubTaxonomyId);
    }
}

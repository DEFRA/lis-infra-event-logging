// <copyright file="EventExtractionRuleConfiguration.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database.Configuration;

using Defra.Lis.EventLogging.Database.Entities;

public sealed class EventExtractionRuleConfiguration : IEntityTypeConfiguration<EventExtractionRule>
{
    public void Configure(EntityTypeBuilder<EventExtractionRule> builder)
    {
        builder.ToTable("event_extraction_rules");
        builder.HasKey(x => x.Id);
        builder.HasAlternateKey(x => new { x.Id, x.SubTaxonomyId, x.ValueType });
        builder.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
        builder.Property(x => x.JsonPath).HasColumnType("jsonpath").IsRequired();
        builder.Property(x => x.ValueType).IsRequired();
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.HasIndex(x => new { x.SubTaxonomyId, x.TokenId }).IsUnique();
        builder.HasOne(x => x.SubTaxonomy).WithMany(x => x.ExtractionRules).HasForeignKey(x => x.SubTaxonomyId);
        builder.HasOne(x => x.Token).WithMany(x => x.Rules).HasForeignKey(x => x.TokenId);
    }
}

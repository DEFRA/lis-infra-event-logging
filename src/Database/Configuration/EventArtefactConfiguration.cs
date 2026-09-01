// <copyright file="EventArtefactConfiguration.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database.Configuration;

using Defra.Lis.EventLogging.Database.Entities;

public sealed class EventArtefactConfiguration : IEntityTypeConfiguration<EventArtefact>
{
    public void Configure(EntityTypeBuilder<EventArtefact> builder)
    {
        builder.ToTable("event_artefacts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
        builder.Property(x => x.MimeType).IsRequired();
        builder.Property(x => x.OriginalFilename).IsRequired();
        builder.Property(x => x.S3Path).IsRequired();
        builder.Property(x => x.ThumbnailMimeType).HasMaxLength(100);
        builder.Property(x => x.ThumbnailStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ThumbnailStatus.Pending)
            .IsRequired();
        builder.Property(x => x.ThumbnailFailureCode).HasMaxLength(100);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.HasIndex(x => x.EventId);
        builder.HasOne(x => x.Event)
            .WithMany(x => x.Artefacts)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

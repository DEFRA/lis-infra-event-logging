// <copyright file="EventSubmissionConfiguration.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database.Configuration;

using Defra.Lis.EventLogging.Database.Entities;

public sealed class EventSubmissionConfiguration : IEntityTypeConfiguration<EventSubmission>
{
    public void Configure(EntityTypeBuilder<EventSubmission> builder)
    {
        builder.ToTable("event_submissions");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.LogId);
        builder.HasIndex(x => x.ArtefactId).IsUnique();
        builder.HasIndex(x => new { x.ClientId, x.IdempotencyKey }).IsUnique();
        builder.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.ClientId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.IdempotencyKey).HasMaxLength(255).IsRequired();
        builder.Property(x => x.RequestFingerprint).HasMaxLength(64).IsRequired();
        builder.Property(x => x.PendingS3Key).HasMaxLength(1024);
        builder.Property(x => x.OriginalFilename).HasMaxLength(255);
        builder.Property(x => x.MimeType).HasMaxLength(255);
        builder.Property(x => x.FailureCode).HasMaxLength(100);
        builder.Property(x => x.SubmittedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
    }
}

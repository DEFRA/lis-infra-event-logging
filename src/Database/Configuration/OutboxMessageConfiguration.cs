// <copyright file="OutboxMessageConfiguration.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database.Configuration;

using Defra.Database.Postgres;
using Defra.Lis.EventLogging.Database.Entities;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.PublishedAt, x.CreatedAt });
        builder.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
        builder.Property(x => x.MessageType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Payload).HasColumnType(ColumnTypes.JsonB).IsRequired();
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.HasOne(x => x.Submission)
            .WithMany(x => x.OutboxMessages)
            .HasForeignKey(x => x.SubmissionId);
    }
}

// <copyright file="SubmissionModelConfigurationTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database.PostgreSQL.Tests;

using Defra.Lis.EventLogging.Database.Configuration;
using Defra.Lis.EventLogging.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using EventEntity = Defra.Lis.EventLogging.Database.Entities.Event;

public class SubmissionModelConfigurationTests
{
    private readonly IModel model;

    public SubmissionModelConfigurationTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql("Host=localhost;Database=event_logging;Username=test;Password=test")
            .Options;

        using var context = new TestDbContext(options);
        this.model = context.Model;
    }

    [Fact]
    public void EventSubmission_Should_Enforce_Client_Idempotency()
    {
        var entity = this.model.FindEntityType(typeof(EventSubmission))!;
        var index = entity.GetIndexes().Single(x =>
            x.Properties.Select(p => p.Name).SequenceEqual(
                [nameof(EventSubmission.ClientId), nameof(EventSubmission.IdempotencyKey)]));

        index.IsUnique.ShouldBeTrue();
    }

    [Fact]
    public void EventSubmission_Should_Reserve_ShortIds_For_Event_Creation()
    {
        var entity = this.model.FindEntityType(typeof(EventSubmission))!;
        var index = entity.GetIndexes().Single(x =>
            x.Properties.Select(p => p.Name).SequenceEqual([nameof(EventSubmission.ShortId)]));

        index.IsUnique.ShouldBeTrue();
        index.GetFilter().ShouldBe("type IN ('CreateEvent', 'CreateEventWithArtefact')");
    }

    [Fact]
    public void EventSubmission_Should_Store_Enums_As_Text()
    {
        var entity = this.model.FindEntityType(typeof(EventSubmission))!;

        entity.FindProperty(nameof(EventSubmission.Type))!.GetMaxLength().ShouldBe(50);
        entity.FindProperty(nameof(EventSubmission.Status))!.GetMaxLength().ShouldBe(50);
    }

    [Fact]
    public void OutboxMessage_Should_Store_Payload_As_Jsonb()
    {
        var entity = this.model.FindEntityType(typeof(OutboxMessage))!;

        entity.FindProperty(nameof(OutboxMessage.Payload))!.GetColumnType().ShouldBe("jsonb");
    }

    [Fact]
    public void OutboxMessage_Should_Require_A_Submission()
    {
        var entity = this.model.FindEntityType(typeof(OutboxMessage))!;
        var foreignKey = entity.GetForeignKeys().Single();

        foreignKey.PrincipalEntityType.ClrType.ShouldBe(typeof(EventSubmission));
        foreignKey.IsRequired.ShouldBeTrue();
    }

    [Fact]
    public void Event_Should_Have_Query_Indexes_And_A_Unique_ShortId()
    {
        var entity = this.model.FindEntityType(typeof(EventEntity))!;
        var indexes = entity.GetIndexes().ToList();
        string[] createdAtIndex = [nameof(EventEntity.CreatedAt), nameof(EventEntity.Id)];
        string[] cphIndex =
        [
            nameof(EventEntity.CountyParishHolding), nameof(EventEntity.CreatedAt), nameof(EventEntity.Id),
        ];

        indexes.Single(x => x.Properties.Select(p => p.Name).SequenceEqual([nameof(EventEntity.ShortId)]))
            .IsUnique.ShouldBeTrue();
        indexes.ShouldContain(x => x.Properties.Select(p => p.Name).SequenceEqual(createdAtIndex));
        indexes.ShouldContain(x => x.Properties.Select(p => p.Name).SequenceEqual(cphIndex));
    }

    [Fact]
    public void ExtractionToken_Name_Should_Be_Unique()
    {
        var entity = this.model.FindEntityType(typeof(EventExtractionToken))!;
        var index = entity.GetIndexes().Single(x =>
            x.Properties.Select(p => p.Name).SequenceEqual([nameof(EventExtractionToken.Name)]));

        index.IsUnique.ShouldBeTrue();
    }

    [Fact]
    public void EventArtefact_Should_Not_Cascade_When_An_Event_Is_Deleted()
    {
        var entity = this.model.FindEntityType(typeof(EventArtefact))!;
        var foreignKey = entity.GetForeignKeys().Single(x => x.PrincipalEntityType.ClrType == typeof(EventEntity));

        foreignKey.DeleteBehavior.ShouldBe(DeleteBehavior.NoAction);
    }

    [Fact]
    public void EventArtefact_Should_Store_Thumbnail_Status_As_Text()
    {
        var entity = this.model.FindEntityType(typeof(EventArtefact))!;
        var status = entity.FindProperty(nameof(EventArtefact.ThumbnailStatus))!;

        status.GetMaxLength().ShouldBe(20);
        status.IsNullable.ShouldBeFalse();
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventSubmissionConfiguration).Assembly);
        }
    }
}

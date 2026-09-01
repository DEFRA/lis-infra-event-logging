// <copyright file="EventQueryRepositoryTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Repositories.Tests.Events;

using System.Text.Json;
using Defra.Database.Postgres;
using Defra.Lis.EventLogging.Database;
using Defra.Lis.EventLogging.Database.Entities;
using Defra.Lis.EventLogging.Models.Requests.Logging;
using Defra.Lis.EventLogging.Repositories.Artefacts;
using Defra.Lis.EventLogging.Repositories.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using EventEntity = Defra.Lis.EventLogging.Database.Entities.Event;

public class EventQueryRepositoryTests
{
    [Fact]
    public async Task QueryAsync_Should_Return_All_Events_When_Cph_Is_Not_Supplied()
    {
        var fixture = await CreateFixtureAsync();
        await using var context = fixture.CreateReadContext();
        var repository = new EventQueryRepository(context);

        var result = await repository.QueryAsync(
            new QueryEvents(),
            TestContext.Current.CancellationToken);

        result.TotalCount.ShouldBe(3);
        result.Items.Select(x => x.Event.ShortId).ShouldBe(["EVT-003", "EVT-002", "EVT-001"]);
    }

    [Fact]
    public async Task QueryAsync_Should_Filter_By_Cph_And_Page()
    {
        var fixture = await CreateFixtureAsync();
        await using var context = fixture.CreateReadContext();
        var repository = new EventQueryRepository(context);

        var result = await repository.QueryAsync(
            new QueryEvents()
            {
                CountyParishHolding = "12/345/6789",
                Page = 2,
                PageSize = 1,
            },
            TestContext.Current.CancellationToken);

        result.TotalCount.ShouldBe(2);
        result.Items.Single().Event.ShortId.ShouldBe("EVT-001");
    }

    [Fact]
    public async Task QueryAsync_Should_Require_All_Token_Filters_By_Default()
    {
        var fixture = await CreateFixtureAsync();
        await using var context = fixture.CreateReadContext();
        var repository = new EventQueryRepository(context);

        var result = await repository.QueryAsync(
            new QueryEvents()
            {
                Filters =
                [
                    new EventTokenFilter() { Token = "ear_tag", Value = "UK-001", },
                    new EventTokenFilter() { Token = "submission_ref", Value = "SUB-001", },
                ],
            },
            TestContext.Current.CancellationToken);

        result.Items.Single().Event.ShortId.ShouldBe("EVT-001");
    }

    [Fact]
    public async Task QueryAsync_Should_Allow_Any_Token_Filter()
    {
        var fixture = await CreateFixtureAsync();
        await using var context = fixture.CreateReadContext();
        var repository = new EventQueryRepository(context);

        var result = await repository.QueryAsync(
            new QueryEvents()
            {
                Match = FilterMatch.Any,
                Filters =
                [
                    new EventTokenFilter() { Token = "ear_tag", Value = "UK-001", },
                    new EventTokenFilter() { Token = "submission_ref", Value = "SUB-002", },
                ],
            },
            TestContext.Current.CancellationToken);

        result.TotalCount.ShouldBe(2);
        result.Items.Select(x => x.Event.ShortId).ShouldBe(["EVT-002", "EVT-001"]);
    }

    [Fact]
    public async Task QueryAsync_Should_Apply_Requested_Sort()
    {
        var fixture = await CreateFixtureAsync();
        await using var context = fixture.CreateReadContext();
        var repository = new EventQueryRepository(context);

        var result = await repository.QueryAsync(
            new QueryEvents()
            {
                SortBy = EventSortBy.Title,
                SortOrder = SortOrder.Ascending,
            },
            TestContext.Current.CancellationToken);

        result.Items.Select(x => x.Event.Title).ShouldBe(["Alpha", "Beta", "Gamma"]);
    }

    [Fact]
    public async Task GetByShortIdAsync_Should_Return_Only_Artefact_References()
    {
        var fixture = await CreateFixtureAsync();
        await using var context = fixture.CreateReadContext();
        var repository = new EventQueryRepository(context);

        var result = await repository.GetByShortIdAsync(
            "EVT-001",
            TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.Artefacts.Count.ShouldBe(1);
        result.Artefacts.Single().Thumbnail.ShouldBe([1, 2, 3]);
        result.Event.Artefacts.ShouldBeEmpty();
    }

    [Fact]
    public async Task ArtefactRepository_Should_Require_The_Event_And_Artefact_To_Match()
    {
        var fixture = await CreateFixtureAsync();
        await using var writeContext = fixture.CreateWriteContext();
        await using var context = fixture.CreateReadContext();
        var queryRepository = new EventQueryRepository(context);
        var artefactRepository = new ArtefactRepository(writeContext, context);
        var eventResult = await queryRepository.GetByShortIdAsync(
            "EVT-001",
            TestContext.Current.CancellationToken);

        var result = await artefactRepository.GetForEventAsync(
            Guid.NewGuid(),
            eventResult!.Artefacts.Single().Id,
            TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    private static async Task<QueryFixture> CreateFixtureAsync()
    {
        var fixture = new QueryFixture(Guid.NewGuid().ToString());
        await using var context = fixture.CreateWriteContext();

        var taxonomy = new EventTaxonomy { Id = Guid.NewGuid(), Name = "BIRTH", };
        var species = new EventSpecies { Id = Guid.NewGuid(), Name = "CTT", };
        var subTaxonomy = new EventSubTaxonomy
        {
            Id = Guid.NewGuid(), Name = "DEFAULT", Taxonomy = taxonomy, Species = species,
        };
        var earTag = new EventExtractionToken { Id = Guid.NewGuid(), Name = "ear_tag", };
        var submissionReference = new EventExtractionToken
        {
            Id = Guid.NewGuid(), Name = "submission_ref",
        };
        var earTagRule = new EventExtractionRule
        {
            Id = Guid.NewGuid(),
            SubTaxonomy = subTaxonomy,
            Token = earTag,
            JsonPath = "$.earTag",
            ValueType = "text",
        };
        var submissionRule = new EventExtractionRule
        {
            Id = Guid.NewGuid(),
            SubTaxonomy = subTaxonomy,
            Token = submissionReference,
            JsonPath = "$.submissionReference",
            ValueType = "text",
        };

        var first = CreateEvent("EVT-001", "Beta", "12/345/6789", subTaxonomy, 1);
        var second = CreateEvent("EVT-002", "Alpha", "12/345/6789", subTaxonomy, 2);
        var third = CreateEvent("EVT-003", "Gamma", "98/765/4321", subTaxonomy, 3);
        first.Artefacts.Add(new EventArtefact()
        {
            Id = Guid.NewGuid(),
            MimeType = "application/pdf",
            OriginalFilename = "evidence.pdf",
            S3Path = "events/evidence.pdf",
            Thumbnail = [1, 2, 3],
        });

        context.AddRange(taxonomy, species, subTaxonomy, earTag, submissionReference, earTagRule, submissionRule);
        context.AddRange(first, second, third);
        context.AddRange(
            CreateValue(first, earTagRule, "UK-001"),
            CreateValue(first, submissionRule, "SUB-001"),
            CreateValue(second, earTagRule, "UK-002"),
            CreateValue(second, submissionRule, "SUB-002"),
            CreateValue(third, earTagRule, "UK-003"));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return fixture;
    }

    private static EventEntity CreateEvent(
        string shortId,
        string title,
        string cph,
        EventSubTaxonomy subTaxonomy,
        int day)
    {
        return new EventEntity()
        {
            Id = Guid.NewGuid(),
            ShortId = shortId,
            CountyParishHolding = cph,
            CreatedAt = new DateTimeOffset(2026, 1, day, 0, 0, 0, TimeSpan.Zero),
            Title = title,
            SubTaxonomy = subTaxonomy,
            CreatedBy = "test-client",
        };
    }

    private static EventExtractedValue CreateValue(
        EventEntity eventEntity,
        EventExtractionRule rule,
        string value)
    {
        return new EventExtractedValue()
        {
            Event = eventEntity,
            ExtractionRule = rule,
            SubTaxonomyId = eventEntity.SubTaxonomy.Id,
            ValueType = "text",
            ValueText = value,
        };
    }

    private sealed class QueryFixture(string databaseName)
    {
        public PostgresDbContext CreateWriteContext()
        {
            return new PostgresDbContext(CreateOptions<PostgresDbContext>());
        }

        public ReadOnlyPostgresDbContext CreateReadContext()
        {
            return new ReadOnlyPostgresDbContext(CreateOptions<ReadOnlyPostgresDbContext>());
        }

        private DbContextOptions<TContext> CreateOptions<TContext>()
            where TContext : DbContext
        {
            return new DbContextOptionsBuilder<TContext>()
                .UseInMemoryDatabase(databaseName)
                .ReplaceService<IModelCustomizer, TestModelCustomizer>()
                .Options;
        }
    }

    private sealed class TestModelCustomizer(ModelCustomizerDependencies dependencies)
        : EventLoggingModelCustomizer(dependencies)
    {
        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);
            modelBuilder.Entity<EventEntity>()
                .Property(x => x.Data)
                .HasConversion(
                    value => value!.RootElement.GetRawText(),
                    value => JsonDocument.Parse(value, default(JsonDocumentOptions)));
            modelBuilder.Entity<EventEntity>().Property(x => x.County).IsRequired(false);
            modelBuilder.Entity<EventEntity>().Property(x => x.Parish).IsRequired(false);
            modelBuilder.Entity<EventEntity>().Property(x => x.Holding).IsRequired(false);
            modelBuilder.Entity<EventExtractedValue>()
                .Property(x => x.ValueJson)
                .HasConversion(
                    value => value!.RootElement.GetRawText(),
                    value => JsonDocument.Parse(value, default(JsonDocumentOptions)));
            modelBuilder.Entity<OutboxMessage>()
                .Property(x => x.Payload)
                .HasConversion(
                    value => value.RootElement.GetRawText(),
                    value => JsonDocument.Parse(value, default(JsonDocumentOptions)));
        }
    }
}

// <copyright file="EventQueryServiceTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services.Tests;

using System.Text.Json;
using Defra.Lis.EventLogging.Database.Entities;
using Defra.Lis.EventLogging.Models.Requests.Logging;
using Defra.Lis.EventLogging.Repositories.Events;
using Defra.Lis.EventLogging.Services;
using NSubstitute;
using EventEntity = Defra.Lis.EventLogging.Database.Entities.Event;

public class EventQueryServiceTests
{
    private readonly IEventQueryRepository repository = Substitute.For<IEventQueryRepository>();

    [Fact]
    public async Task QueryEventsAsync_Should_Map_Page_And_Event_Data()
    {
        var entity = CreateEvent();
        var artefactId = entity.Artefacts.Single().Id;
        var request = new QueryEvents() { Page = 2, PageSize = 10, };
        this.repository.QueryAsync(request, Arg.Any<CancellationToken>())
            .Returns(new EventQueryPage()
            {
                Items =
                [
                    new EventQueryItem()
                    {
                        Event = entity,
                        Artefacts =
                        [
                            new ArtefactQueryReference()
                            {
                                Id = artefactId,
                                Thumbnail = [1, 2, 3],
                                ThumbnailMimeType = "image/webp",
                                ThumbnailWidth = 320,
                                ThumbnailHeight = 180,
                                ThumbnailStatus = ThumbnailStatus.Available,
                            },
                        ],
                    },
                ],
                TotalCount = 11,
            });
        var service = new EventQueryService(this.repository);

        var result = await service.QueryEventsAsync(request, TestContext.Current.CancellationToken);

        result.Page.ShouldBe(2);
        result.PageSize.ShouldBe(10);
        result.TotalCount.ShouldBe(11);
        result.TotalPages.ShouldBe(2);
        var item = result.Items.Single();
        item.LogId.ShouldBe(entity.Id);
        item.ShortId.ShouldBe(entity.ShortId);
        item.SubTaxonomyId.ShouldBe(entity.SubTaxonomyId);
        item.Data!.RootElement.GetProperty("reference").GetString().ShouldBe("SUB-1");
        item.Artefacts.Single().Id.ShouldBe(artefactId);
        var thumbnail = item.Artefacts.Single().Thumbnail.ShouldNotBeNull();
        thumbnail.Content.ShouldBe([1, 2, 3]);
        thumbnail.MimeType.ShouldBe("image/webp");
        thumbnail.Width.ShouldBe(320);
        thumbnail.Height.ShouldBe(180);
    }

    [Fact]
    public async Task GetEventAsync_Should_Return_Null_When_Not_Found()
    {
        this.repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((EventQueryItem?)null);
        var service = new EventQueryService(this.repository);

        var result = await service.GetEventAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetEventByShortIdAsync_Should_Map_The_Event()
    {
        var entity = CreateEvent();
        this.repository.GetByShortIdAsync(entity.ShortId, Arg.Any<CancellationToken>())
            .Returns(new EventQueryItem() { Event = entity, Artefacts = [], });
        var service = new EventQueryService(this.repository);

        var result = await service.GetEventByShortIdAsync(
            entity.ShortId,
            TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.ShortId.ShouldBe(entity.ShortId);
    }

    private static EventEntity CreateEvent()
    {
        return new EventEntity()
        {
            Id = Guid.NewGuid(),
            ShortId = "EVT-ABC123",
            CountyParishHolding = "12/345/6789",
            CreatedAt = DateTimeOffset.UtcNow,
            Title = "Birth event",
            SubTaxonomyId = Guid.NewGuid(),
            Data = JsonDocument.Parse("""{"reference":"SUB-1"}"""),
            CreatedBy = "test-client",
            Artefacts =
            [
                new EventArtefact()
                {
                    Id = Guid.NewGuid(),
                    MimeType = "application/pdf",
                    OriginalFilename = "evidence.pdf",
                    S3Path = "events/evidence.pdf",
                    CreatedAt = DateTimeOffset.UtcNow,
                },
            ],
        };
    }
}

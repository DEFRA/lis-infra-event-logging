// <copyright file="EventQueryService.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services;

using Defra.Lis.EventLogging.Database.Entities;
using Defra.Lis.EventLogging.Models.Requests.Logging;
using Defra.Lis.EventLogging.Models.Responses.Logging;
using Defra.Lis.EventLogging.Repositories.Events;

public class EventQueryService(IEventQueryRepository repository) : IEventQueryService
{
    public async Task<PagedEventResult> QueryEventsAsync(
        QueryEvents request,
        CancellationToken cancellationToken = default)
    {
        var result = await repository.QueryAsync(request, cancellationToken);

        return new PagedEventResult()
        {
            Items = result.Items.Select(MapEvent).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = result.TotalCount,
        };
    }

    public async Task<EventResult?> GetEventAsync(
        Guid logId,
        CancellationToken cancellationToken = default)
    {
        var result = await repository.GetByIdAsync(logId, cancellationToken);
        return result is null ? null : MapEvent(result);
    }

    public async Task<EventResult?> GetEventByUrlShortCodeAsync(
        string urlShortCode,
        CancellationToken cancellationToken = default)
    {
        var result = await repository.GetByUrlShortCodeAsync(urlShortCode, cancellationToken);
        return result is null ? null : MapEvent(result);
    }

    private static EventResult MapEvent(EventQueryItem item)
    {
        var entity = item.Event;

        return new EventResult()
        {
            LogId = entity.Id,
            UrlShortCode = entity.UrlShortCode,
            CountyParishHolding = entity.CountyParishHolding,
            CreatedAt = entity.CreatedAt,
            Title = entity.Title,
            SubTaxonomyId = entity.SubTaxonomyId,
            Data = entity.Data,
            CreatedBy = entity.CreatedBy,
            Artefacts = item.Artefacts.Select(x => new EventArtefactReference()
            {
                Id = x.Id,
                Thumbnail = x.ThumbnailStatus == ThumbnailStatus.Available &&
                    x.Thumbnail is not null &&
                    x.ThumbnailMimeType is not null &&
                    x.ThumbnailWidth is not null &&
                    x.ThumbnailHeight is not null
                    ? new EventThumbnail()
                    {
                        Content = x.Thumbnail,
                        MimeType = x.ThumbnailMimeType,
                        Width = x.ThumbnailWidth.Value,
                        Height = x.ThumbnailHeight.Value,
                    }
                    : null,
            }).ToList(),
        };
    }
}

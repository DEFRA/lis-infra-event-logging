// <copyright file="IEventQueryService.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services;

using Defra.Lis.EventLogging.Models.Requests.Logging;
using Defra.Lis.EventLogging.Models.Responses.Logging;

public interface IEventQueryService
{
    Task<PagedEventResult> QueryEventsAsync(
        QueryEvents request,
        CancellationToken cancellationToken = default);

    Task<EventResult?> GetEventAsync(
        Guid logId,
        CancellationToken cancellationToken = default);

    Task<EventResult?> GetEventByUrlShortCodeAsync(
        string urlShortCode,
        CancellationToken cancellationToken = default);
}

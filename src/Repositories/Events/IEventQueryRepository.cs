// <copyright file="IEventQueryRepository.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Repositories.Events;

using Defra.Lis.EventLogging.Models.Requests.Logging;

public interface IEventQueryRepository
{
    Task<EventQueryPage> QueryAsync(
        QueryEvents request,
        CancellationToken cancellationToken = default);

    Task<EventQueryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EventQueryItem?> GetByShortIdAsync(string shortId, CancellationToken cancellationToken = default);
}

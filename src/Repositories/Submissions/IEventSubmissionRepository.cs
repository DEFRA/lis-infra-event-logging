// <copyright file="IEventSubmissionRepository.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Repositories.Submissions;

using Defra.Lis.EventLogging.Database.Entities;

public interface IEventSubmissionRepository
{
    Task<EventSubmission?> GetByIdempotencyKeyAsync(
        string clientId,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<EventSubmission?> GetByIdAsync(
        Guid submissionId,
        CancellationToken cancellationToken = default);

    Task<Guid?> ResolveSubTaxonomyIdAsync(
        string species,
        string taxonomy,
        string subTaxonomy,
        CancellationToken cancellationToken = default);

    Task<string?> GetEventShortIdAsync(Guid eventId, CancellationToken cancellationToken = default);

    Task CreateAsync(
        EventSubmission submission,
        OutboxMessage outboxMessage,
        CancellationToken cancellationToken = default);
}

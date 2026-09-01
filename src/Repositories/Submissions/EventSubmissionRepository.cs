// <copyright file="EventSubmissionRepository.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Repositories.Submissions;

using Defra.Database.Postgres;
using Defra.Lis.EventLogging.Database.Entities;
using EventEntity = Defra.Lis.EventLogging.Database.Entities.Event;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class EventSubmissionRepository(
    PostgresDbContext writeContext,
    ReadOnlyPostgresDbContext readContext) : IEventSubmissionRepository
{
    public Task<EventSubmission?> GetByIdempotencyKeyAsync(
        string clientId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        return readContext.Set<EventSubmission>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.ClientId == clientId && x.IdempotencyKey == idempotencyKey,
                cancellationToken);
    }

    public Task<EventSubmission?> GetByIdAsync(
        Guid submissionId,
        CancellationToken cancellationToken = default)
    {
        return readContext.Set<EventSubmission>()
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == submissionId, cancellationToken);
    }

    public Task<Guid?> ResolveSubTaxonomyIdAsync(
        string species,
        string taxonomy,
        string subTaxonomy,
        CancellationToken cancellationToken = default)
    {
        return readContext.Set<EventSubTaxonomy>()
            .AsNoTracking()
            .Where(x =>
                x.Species.Name == species &&
                x.Taxonomy.Name == taxonomy &&
                x.Name == subTaxonomy)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<string?> GetEventShortIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return readContext.Set<EventEntity>()
            .AsNoTracking()
            .Where(x => x.Id == eventId)
            .Select(x => x.ShortId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task CreateAsync(
        EventSubmission submission,
        OutboxMessage outboxMessage,
        CancellationToken cancellationToken = default)
    {
        writeContext.Add(submission);
        writeContext.Add(outboxMessage);
        await writeContext.SaveChangesAsync(cancellationToken);
    }
}

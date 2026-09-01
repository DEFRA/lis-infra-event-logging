// <copyright file="EventSubmissionProcessingRepository.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Repositories.Submissions;

using Defra.Database.Postgres;
using Defra.Lis.EventLogging.Database.Domain;
using Defra.Lis.EventLogging.Database.Entities;
using Defra.Lis.EventLogging.Models.Messages;
using EventEntity = Defra.Lis.EventLogging.Database.Entities.Event;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class EventSubmissionProcessingRepository(PostgresDbContext context)
    : IEventSubmissionProcessingRepository
{
    public async Task<IReadOnlyList<OutboxMessage>> GetUnpublishedAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<OutboxMessage>()
            .AsNoTracking()
            .Where(x => x.PublishedAt == null)
            .OrderBy(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkPublishedAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var message = await context.Set<OutboxMessage>().SingleAsync(x => x.Id == messageId, cancellationToken);
        message.PublishedAt = DateTimeOffset.UtcNow;
        message.AttemptCount++;
        message.LastError = null;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkPublishFailedAsync(
        Guid messageId,
        string error,
        CancellationToken cancellationToken = default)
    {
        var message = await context.Set<OutboxMessage>().SingleAsync(x => x.Id == messageId, cancellationToken);
        message.AttemptCount++;
        message.LastError = error[..Math.Min(error.Length, 2000)];
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> CompleteAsync(
        EventSubmissionMessage message,
        CancellationToken cancellationToken = default)
    {
        var submission = await context.Set<EventSubmission>()
            .SingleAsync(x => x.Id == message.SubmissionId, cancellationToken);
        if (submission.Status == SubmissionStatus.Completed)
        {
            return false;
        }

        submission.Status = SubmissionStatus.Processing;
        submission.ProcessingStartedAt ??= DateTimeOffset.UtcNow;
        submission.UpdatedAt = DateTimeOffset.UtcNow;

        if (message.MessageType is nameof(SubmissionType.CreateEvent) or nameof(SubmissionType.CreateEventWithArtefact) &&
            !await context.Set<EventEntity>().AnyAsync(x => x.Id == message.LogId, cancellationToken))
        {
            context.Add(new EventEntity()
            {
                Id = message.LogId,
                ShortId = message.ShortId,
                CountyParishHolding = message.CountyParishHolding ??
                    throw new InvalidDataException("County parish holding is required."),
                CreatedAt = message.CreatedAt ?? DateTimeOffset.UtcNow,
                Title = message.Title ?? throw new InvalidDataException("Title is required."),
                Data = message.Data,
                CreatedBy = message.CreatedBy ?? throw new InvalidDataException("Created by is required."),
                SubTaxonomyId = message.SubTaxonomyId ??
                    throw new InvalidDataException("Sub-taxonomy is required."),
            });
        }

        if (message.ArtefactId is not null &&
            !await context.Set<EventArtefact>().AnyAsync(x => x.Id == message.ArtefactId, cancellationToken))
        {
            context.Add(new EventArtefact()
            {
                Id = message.ArtefactId.Value,
                EventId = message.LogId,
                MimeType = message.MimeType ?? throw new InvalidDataException("MIME type is required."),
                OriginalFilename = message.OriginalFilename ??
                    throw new InvalidDataException("Original filename is required."),
                S3Path = message.PendingS3Key ?? throw new InvalidDataException("S3 key is required."),
                ThumbnailStatus = ThumbnailStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow,
            });
        }

        submission.Status = SubmissionStatus.Completed;
        submission.CompletedAt = DateTimeOffset.UtcNow;
        submission.UpdatedAt = DateTimeOffset.UtcNow;
        submission.FailureCode = null;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task MarkSubmissionFailedAsync(
        Guid submissionId,
        string failureCode,
        CancellationToken cancellationToken = default)
    {
        context.ChangeTracker.Clear();
        var submission = await context.Set<EventSubmission>()
            .SingleAsync(x => x.Id == submissionId, cancellationToken);
        submission.Status = SubmissionStatus.Failed;
        submission.FailureCode = failureCode;
        submission.UpdatedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }
}

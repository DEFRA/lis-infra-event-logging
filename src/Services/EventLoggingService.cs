// <copyright file="EventLoggingService.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Defra.Lis.EventLogging.Database.Domain;
using Defra.Lis.EventLogging.Database.Entities;
using Defra.Lis.EventLogging.Models.Messages;
using Defra.Lis.EventLogging.Models.Requests.Logging;
using Defra.Lis.EventLogging.Models.Responses.Logging;
using Defra.Lis.EventLogging.Repositories.Artefacts;
using Defra.Lis.EventLogging.Repositories.Submissions;
using Defra.Lis.EventLogging.Services.Models;
using DatabaseSubmissionStatus = Defra.Lis.EventLogging.Database.Domain.SubmissionStatus;
using ModelSubmissionStatus = Defra.Lis.EventLogging.Models.Responses.Logging.SubmissionStatus;

public class EventLoggingService(
    IEventSubmissionRepository repository,
    IArtefactStore artefactStore) : IEventLoggingService
{
    public async Task<EventSubmissionResult> SubmitEventAsync(
        PostEvent request,
        SubmissionContext context,
        CancellationToken cancellationToken = default)
    {
        var fingerprint = CreateFingerprint(request);
        var existing = await GetExistingAsync(context, fingerprint, cancellationToken);
        if (existing is not null)
        {
            return MapResult(existing);
        }

        var subTaxonomyId = await ResolveSubTaxonomyIdAsync(request, cancellationToken);
        var submission = CreateSubmission(SubmissionType.CreateEvent, context, fingerprint);
        var message = CreateEventMessage(submission, request, subTaxonomyId);
        await repository.CreateAsync(submission, CreateOutboxMessage(submission, message), cancellationToken);
        return MapResult(submission);
    }

    public async Task<EventSubmissionResult> SubmitEventWithArtefactAsync(
        PostEventWithArtefact request,
        SubmissionContext context,
        CancellationToken cancellationToken = default)
    {
        var fingerprint = CreateFingerprint(request.Event, request.Artefact);
        var existing = await GetExistingAsync(context, fingerprint, cancellationToken);
        if (existing is not null)
        {
            return MapResult(existing);
        }

        var subTaxonomyId = await ResolveSubTaxonomyIdAsync(request.Event, cancellationToken);
        var submission = CreateSubmission(SubmissionType.CreateEventWithArtefact, context, fingerprint, true);
        await StageAndPersistAsync(submission, request.Event, request.Artefact, subTaxonomyId, cancellationToken);
        return MapResult(submission);
    }

    public async Task<EventSubmissionResult> SubmitArtefactAsync(
        Guid logId,
        PostArtefact request,
        SubmissionContext context,
        CancellationToken cancellationToken = default)
    {
        var fingerprint = CreateFingerprint(logId, request);
        var existing = await GetExistingAsync(context, fingerprint, cancellationToken);
        if (existing is not null)
        {
            return MapResult(existing);
        }

        var shortId = await repository.GetEventShortIdAsync(logId, cancellationToken)
            ?? throw new ArgumentException($"Event '{logId}' does not exist.");
        var submission = CreateSubmission(SubmissionType.AddArtefact, context, fingerprint, true, logId, shortId);
        await StageAndPersistAsync(submission, null, request, null, cancellationToken);
        return MapResult(submission);
    }

    public async Task<EventSubmissionStatusResult?> GetSubmissionStatusAsync(
        Guid submissionId,
        CancellationToken cancellationToken = default)
    {
        var submission = await repository.GetByIdAsync(submissionId, cancellationToken);
        return submission is null ? null : MapStatusResult(submission);
    }

    private static EventSubmission CreateSubmission(
        SubmissionType type,
        SubmissionContext context,
        string fingerprint,
        bool hasArtefact = false,
        Guid? logId = null,
        string? shortId = null)
    {
        var now = DateTimeOffset.UtcNow;
        return new EventSubmission()
        {
            Id = Guid.NewGuid(),
            Type = type,
            Status = DatabaseSubmissionStatus.Pending,
            LogId = logId ?? Guid.NewGuid(),
            ArtefactId = hasArtefact ? Guid.NewGuid() : null,
            ShortId = shortId ?? $"EVT-{Convert.ToHexString(RandomNumberGenerator.GetBytes(6))}",
            ClientId = context.ClientId,
            IdempotencyKey = context.IdempotencyKey,
            RequestFingerprint = fingerprint,
            CorrelationId = context.CorrelationId,
            SubmittedAt = now,
            UpdatedAt = now,
        };
    }

    private static EventSubmissionMessage CreateEventMessage(
        EventSubmission submission,
        PostEvent request,
        Guid subTaxonomyId)
    {
        return new EventSubmissionMessage()
        {
            MessageType = submission.Type.ToString(),
            SubmissionId = submission.Id,
            LogId = submission.LogId,
            ArtefactId = submission.ArtefactId,
            ShortId = submission.ShortId,
            CountyParishHolding = request.CountyParishHolding,
            CreatedAt = request.CreatedAt,
            Title = request.Title,
            Data = request.Data,
            CreatedBy = request.CreatedBy,
            SubTaxonomyId = subTaxonomyId,
            PendingS3Key = submission.PendingS3Key,
            OriginalFilename = submission.OriginalFilename,
            MimeType = submission.MimeType,
        };
    }

    private static OutboxMessage CreateOutboxMessage(
        EventSubmission submission,
        EventSubmissionMessage message)
    {
        return new OutboxMessage()
        {
            Id = Guid.NewGuid(),
            SubmissionId = submission.Id,
            MessageType = message.MessageType,
            SchemaVersion = message.SchemaVersion,
            Payload = JsonSerializer.SerializeToDocument(message),
            CreatedAt = DateTimeOffset.UtcNow,
            Submission = submission,
        };
    }

    private static string CreateFingerprint(PostEvent request, PostArtefact? artefact = null)
    {
        var value = JsonSerializer.Serialize(new
        {
            Event = request,
            Artefact = artefact is null ? null : new
            {
                artefact.MimeType, artefact.OriginalFilename, artefact.Size,
            },
        });
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    private static string CreateFingerprint(Guid logId, PostArtefact artefact)
    {
        var value = JsonSerializer.Serialize(new
        {
            LogId = logId, artefact.MimeType, artefact.OriginalFilename, artefact.Size,
        });
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    private static EventSubmissionResult MapResult(EventSubmission submission)
    {
        return new EventSubmissionResult()
        {
            SubmissionId = submission.Id,
            LogId = submission.LogId,
            ArtefactId = submission.ArtefactId,
            ShortId = submission.ShortId,
            Status = Enum.Parse<ModelSubmissionStatus>(submission.Status.ToString()),
        };
    }

    private static EventSubmissionStatusResult MapStatusResult(EventSubmission submission)
    {
        return new EventSubmissionStatusResult()
        {
            SubmissionId = submission.Id,
            LogId = submission.LogId,
            ArtefactId = submission.ArtefactId,
            ShortId = submission.ShortId,
            Status = Enum.Parse<ModelSubmissionStatus>(submission.Status.ToString()),
            SubmittedAt = submission.SubmittedAt,
            CompletedAt = submission.CompletedAt,
            FailureCode = submission.FailureCode,
        };
    }

    private static EventSubmissionMessage CreateArtefactMessage(EventSubmission submission)
    {
        return new EventSubmissionMessage()
        {
            MessageType = submission.Type.ToString(),
            SubmissionId = submission.Id,
            LogId = submission.LogId,
            ArtefactId = submission.ArtefactId,
            ShortId = submission.ShortId,
            PendingS3Key = submission.PendingS3Key,
            OriginalFilename = submission.OriginalFilename,
            MimeType = submission.MimeType,
        };
    }

    private async Task<EventSubmission?> GetExistingAsync(
        SubmissionContext context,
        string fingerprint,
        CancellationToken cancellationToken)
    {
        var existing = await repository.GetByIdempotencyKeyAsync(
            context.ClientId,
            context.IdempotencyKey,
            cancellationToken);
        if (existing is not null && existing.RequestFingerprint != fingerprint)
        {
            throw new ArgumentException("The idempotency key has already been used for a different request.");
        }

        return existing;
    }

    private async Task<Guid> ResolveSubTaxonomyIdAsync(
        PostEvent request,
        CancellationToken cancellationToken)
    {
        return await repository.ResolveSubTaxonomyIdAsync(
            request.Species!, request.Taxonomy!, request.SubTaxonomy!, cancellationToken)
            ?? throw new ArgumentException("The supplied species, taxonomy and sub-taxonomy are not valid.");
    }

    private async Task StageAndPersistAsync(
        EventSubmission submission,
        PostEvent? eventRequest,
        PostArtefact artefact,
        Guid? subTaxonomyId,
        CancellationToken cancellationToken)
    {
        var objectKey = $"{submission.LogId:D}/{submission.ArtefactId:D}";
        submission.PendingS3Key = objectKey;
        submission.OriginalFilename = artefact.OriginalFilename;
        submission.MimeType = artefact.MimeType;
        await artefactStore.PutAsync(objectKey, artefact.Content, artefact.MimeType, cancellationToken);

        try
        {
            var message = eventRequest is null
                ? CreateArtefactMessage(submission)
                : CreateEventMessage(submission, eventRequest, subTaxonomyId!.Value);
            await repository.CreateAsync(
                submission, CreateOutboxMessage(submission, message), cancellationToken);
        }
        catch
        {
            await artefactStore.DeleteAsync(objectKey, CancellationToken.None);
            throw;
        }
    }
}

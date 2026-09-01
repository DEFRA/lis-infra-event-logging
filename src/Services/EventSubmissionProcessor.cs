// <copyright file="EventSubmissionProcessor.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services;

using Defra.Lis.EventLogging.Models.Messages;
using Defra.Lis.EventLogging.Repositories.Submissions;

public class EventSubmissionProcessor(
    IEventSubmissionProcessingRepository repository,
    IArtefactThumbnailProcessor thumbnailProcessor) : IEventSubmissionProcessor
{
    public async Task ProcessAsync(
        EventSubmissionMessage message,
        CancellationToken cancellationToken = default)
    {
        if (message.SchemaVersion != 1)
        {
            throw new InvalidDataException($"Unsupported submission schema version '{message.SchemaVersion}'.");
        }

        try
        {
            await repository.CompleteAsync(message, cancellationToken);
        }
        catch
        {
            await repository.MarkSubmissionFailedAsync(
                message.SubmissionId,
                "persistence_failed",
                CancellationToken.None);
            throw;
        }

        if (message.ArtefactId is not null)
        {
            await thumbnailProcessor.ProcessAsync(message.ArtefactId.Value, cancellationToken);
        }
    }
}

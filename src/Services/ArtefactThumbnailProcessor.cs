// <copyright file="ArtefactThumbnailProcessor.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services;

using Defra.Lis.EventLogging.Database.Entities;
using Defra.Lis.EventLogging.Repositories.Artefacts;
using Defra.Lis.EventLogging.Services.Thumbnails;

public class ArtefactThumbnailProcessor(
    IArtefactRepository repository,
    IArtefactStore store,
    IThumbnailService thumbnailService) : IArtefactThumbnailProcessor
{
    public async Task ProcessAsync(Guid artefactId, CancellationToken cancellationToken = default)
    {
        var artefact = await repository.GetForThumbnailAsync(artefactId, cancellationToken) ??
            throw new InvalidOperationException($"Artefact '{artefactId}' was not found.");

        if (artefact.Status is ThumbnailStatus.Available or ThumbnailStatus.Unsupported)
        {
            return;
        }

        if (!thumbnailService.Supports(artefact.MimeType))
        {
            await repository.SetThumbnailStatusAsync(
                artefactId,
                ThumbnailStatus.Unsupported,
                "unsupported_media_type",
                cancellationToken);
            return;
        }

        try
        {
            var storedArtefact = await store.GetAsync(artefact.S3Path, cancellationToken) ??
                throw new InvalidOperationException("The artefact object was not found in storage.");
            await using var content = storedArtefact.Content;
            var generated = await thumbnailService.GenerateAsync(
                artefact.MimeType,
                content,
                cancellationToken);

            await repository.SaveThumbnailAsync(
                artefactId,
                new ThumbnailPersistence()
                {
                    Content = generated.Content,
                    MimeType = generated.MimeType,
                    Width = generated.Width,
                    Height = generated.Height,
                },
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            await repository.SetThumbnailStatusAsync(
                artefactId,
                ThumbnailStatus.Failed,
                "generation_failed",
                CancellationToken.None);
            throw;
        }
    }
}

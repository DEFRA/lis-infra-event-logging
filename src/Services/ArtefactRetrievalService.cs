// <copyright file="ArtefactRetrievalService.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services;

using Defra.Lis.EventLogging.Repositories.Artefacts;
using Defra.Lis.EventLogging.Services.Models;

public class ArtefactRetrievalService(
    IArtefactRepository repository,
    IArtefactStore store) : IArtefactRetrievalService
{
    public async Task<ArtefactDownload?> GetArtefactAsync(
        Guid eventId,
        Guid artefactId,
        CancellationToken cancellationToken = default)
    {
        var reference = await repository.GetForEventAsync(eventId, artefactId, cancellationToken);

        if (reference is null)
        {
            return null;
        }

        var storedArtefact = await store.GetAsync(reference.S3Path, cancellationToken);

        return storedArtefact is null
            ? null
            : new ArtefactDownload()
            {
                Content = storedArtefact.Content,
                ContentLength = storedArtefact.ContentLength,
                MimeType = reference.MimeType,
                Filename = reference.OriginalFilename,
            };
    }
}

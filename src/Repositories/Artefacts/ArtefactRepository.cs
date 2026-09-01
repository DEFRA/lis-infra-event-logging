// <copyright file="ArtefactRepository.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Repositories.Artefacts;

using Defra.Database.Postgres;
using Defra.Lis.EventLogging.Database.Entities;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class ArtefactRepository(
    PostgresDbContext writeContext,
    ReadOnlyPostgresDbContext readContext) : IArtefactRepository
{
    public Task<ArtefactStorageReference?> GetForEventAsync(
        Guid eventId,
        Guid artefactId,
        CancellationToken cancellationToken = default)
    {
        return readContext.Set<EventArtefact>()
            .AsNoTracking()
            .Where(x => x.EventId == eventId && x.Id == artefactId)
            .Select(x => new ArtefactStorageReference()
            {
                Id = x.Id,
                S3Path = x.S3Path,
                MimeType = x.MimeType,
                OriginalFilename = x.OriginalFilename,
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<ArtefactThumbnailReference?> GetForThumbnailAsync(
        Guid artefactId,
        CancellationToken cancellationToken = default)
    {
        return readContext.Set<EventArtefact>()
            .AsNoTracking()
            .Where(x => x.Id == artefactId)
            .Select(x => new ArtefactThumbnailReference()
            {
                Id = x.Id,
                S3Path = x.S3Path,
                MimeType = x.MimeType,
                Status = x.ThumbnailStatus,
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task SaveThumbnailAsync(
        Guid artefactId,
        ThumbnailPersistence thumbnail,
        CancellationToken cancellationToken = default)
    {
        var artefact = await writeContext.Set<EventArtefact>()
            .SingleAsync(x => x.Id == artefactId, cancellationToken);

        artefact.Thumbnail = thumbnail.Content;
        artefact.ThumbnailMimeType = thumbnail.MimeType;
        artefact.ThumbnailWidth = thumbnail.Width;
        artefact.ThumbnailHeight = thumbnail.Height;
        artefact.ThumbnailStatus = ThumbnailStatus.Available;
        artefact.ThumbnailFailureCode = null;
        await writeContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SetThumbnailStatusAsync(
        Guid artefactId,
        ThumbnailStatus status,
        string? failureCode,
        CancellationToken cancellationToken = default)
    {
        var artefact = await writeContext.Set<EventArtefact>()
            .SingleAsync(x => x.Id == artefactId, cancellationToken);

        artefact.ThumbnailStatus = status;
        artefact.ThumbnailFailureCode = failureCode;
        await writeContext.SaveChangesAsync(cancellationToken);
    }
}

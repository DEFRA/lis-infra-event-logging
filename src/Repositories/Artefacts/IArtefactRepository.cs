// <copyright file="IArtefactRepository.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Repositories.Artefacts;

using Defra.Lis.EventLogging.Database.Entities;

public interface IArtefactRepository
{
    Task<ArtefactStorageReference?> GetForEventAsync(
        Guid eventId,
        Guid artefactId,
        CancellationToken cancellationToken = default);

    Task<ArtefactThumbnailReference?> GetForThumbnailAsync(
        Guid artefactId,
        CancellationToken cancellationToken = default);

    Task SaveThumbnailAsync(
        Guid artefactId,
        ThumbnailPersistence thumbnail,
        CancellationToken cancellationToken = default);

    Task SetThumbnailStatusAsync(
        Guid artefactId,
        ThumbnailStatus status,
        string? failureCode,
        CancellationToken cancellationToken = default);
}

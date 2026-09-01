// <copyright file="IArtefactRetrievalService.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services;

using Defra.Lis.EventLogging.Services.Models;

public interface IArtefactRetrievalService
{
    Task<ArtefactDownload?> GetArtefactAsync(
        Guid eventId,
        Guid artefactId,
        CancellationToken cancellationToken = default);
}

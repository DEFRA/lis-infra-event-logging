// <copyright file="IArtefactThumbnailProcessor.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services;

public interface IArtefactThumbnailProcessor
{
    Task ProcessAsync(Guid artefactId, CancellationToken cancellationToken = default);
}

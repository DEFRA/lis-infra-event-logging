// <copyright file="IArtefactStore.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Repositories.Artefacts;

public interface IArtefactStore
{
    Task PutAsync(
        string objectKey,
        Stream content,
        string mimeType,
        CancellationToken cancellationToken = default);

    Task<StoredArtefact?> GetAsync(string objectKey, CancellationToken cancellationToken = default);

    Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default);
}

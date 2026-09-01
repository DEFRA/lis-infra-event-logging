// <copyright file="ArtefactStorageOptions.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Repositories.Artefacts;

public class ArtefactStorageOptions
{
    public const string SectionName = "ArtefactStorage";

    public string BucketName { get; set; } = string.Empty;
}

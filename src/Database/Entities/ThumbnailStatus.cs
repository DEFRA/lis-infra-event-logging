// <copyright file="ThumbnailStatus.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database.Entities;

public enum ThumbnailStatus
{
    Pending,
    Available,
    Unsupported,
    Failed,
}

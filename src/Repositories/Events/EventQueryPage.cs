// <copyright file="EventQueryPage.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Repositories.Events;

public record EventQueryPage
{
    public required IReadOnlyCollection<EventQueryItem> Items { get; init; }

    public required long TotalCount { get; init; }
}

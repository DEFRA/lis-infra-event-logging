// <copyright file="PagedEventResult.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Responses.Logging;

public record PagedEventResult
{
    public required IReadOnlyCollection<EventResult> Items { get; init; }

    public required int Page { get; init; }

    public required int PageSize { get; init; }

    public required long TotalCount { get; init; }

    public int TotalPages => TotalCount == 0
        ? 0
        : (int)Math.Ceiling((double)TotalCount / PageSize);
}

// <copyright file="QueryEvents.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Requests.Logging;

public record QueryEvents
{
    public string? CountyParishHolding { get; init; }

    public IReadOnlyCollection<EventTokenFilter> Filters { get; init; } = [];

    public FilterMatch Match { get; init; } = FilterMatch.All;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 25;

    public EventSortBy SortBy { get; init; } = EventSortBy.CreatedAt;

    public SortOrder SortOrder { get; init; } = SortOrder.Descending;
}

// <copyright file="EventQueryRepository.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Repositories.Events;

using System.Globalization;
using System.Linq.Expressions;
using Defra.Database.Postgres;
using Defra.Lis.EventLogging.Database.Entities;
using Defra.Lis.EventLogging.Models.Requests.Logging;
using EventEntity = Defra.Lis.EventLogging.Database.Entities.Event;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class EventQueryRepository(ReadOnlyPostgresDbContext context) : IEventQueryRepository
{
    public async Task<EventQueryPage> QueryAsync(
        QueryEvents request,
        CancellationToken cancellationToken = default)
    {
        var query = context.Set<EventEntity>().AsNoTracking();

        if (request.CountyParishHolding is not null)
        {
            query = query.Where(x => x.CountyParishHolding == request.CountyParishHolding);
        }

        if (request.Filters.Count > 0)
        {
            var resolvedFilters = await ResolveFiltersAsync(request.Filters, cancellationToken);
            var predicates = resolvedFilters.Select(x => BuildFilterPredicate(x.Filter, x.ValueType)).ToList();

            query = request.Match == FilterMatch.All
                ? predicates.Aggregate(query, (current, predicate) => current.Where(predicate))
                : query.Where(predicates.Aggregate(OrElse));
        }

        var totalCount = await query.LongCountAsync(cancellationToken);
        query = ApplySorting(query, request.SortBy, request.SortOrder);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new EventQueryItem()
            {
                Event = x,
                Artefacts = x.Artefacts.Select(a => new ArtefactQueryReference()
                {
                    Id = a.Id,
                    Thumbnail = a.Thumbnail,
                    ThumbnailMimeType = a.ThumbnailMimeType,
                    ThumbnailWidth = a.ThumbnailWidth,
                    ThumbnailHeight = a.ThumbnailHeight,
                    ThumbnailStatus = a.ThumbnailStatus,
                }).ToList(),
            })
            .ToListAsync(cancellationToken);

        return new EventQueryPage() { Items = items, TotalCount = totalCount, };
    }

    public Task<EventQueryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Set<EventEntity>()
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new EventQueryItem()
            {
                Event = x,
                Artefacts = x.Artefacts.Select(a => new ArtefactQueryReference()
                {
                    Id = a.Id,
                    Thumbnail = a.Thumbnail,
                    ThumbnailMimeType = a.ThumbnailMimeType,
                    ThumbnailWidth = a.ThumbnailWidth,
                    ThumbnailHeight = a.ThumbnailHeight,
                    ThumbnailStatus = a.ThumbnailStatus,
                }).ToList(),
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<EventQueryItem?> GetByUrlShortCodeAsync(
        string urlShortCode,
        CancellationToken cancellationToken = default)
    {
        return context.Set<EventEntity>()
            .AsNoTracking()
            .Where(x => x.UrlShortCode == urlShortCode)
            .Select(x => new EventQueryItem()
            {
                Event = x,
                Artefacts = x.Artefacts.Select(a => new ArtefactQueryReference()
                {
                    Id = a.Id,
                    Thumbnail = a.Thumbnail,
                    ThumbnailMimeType = a.ThumbnailMimeType,
                    ThumbnailWidth = a.ThumbnailWidth,
                    ThumbnailHeight = a.ThumbnailHeight,
                    ThumbnailStatus = a.ThumbnailStatus,
                }).ToList(),
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    private static Expression<Func<EventEntity, bool>> BuildFilterPredicate(
        EventTokenFilter filter,
        string valueType)
    {
        return valueType switch
        {
            "text" => e => e.ExtractedValues.Any(v =>
                v.ExtractionRule.Token.Name == filter.Token && v.ValueText == filter.Value),
            "number" when decimal.TryParse(filter.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
                => e => e.ExtractedValues.Any(v =>
                    v.ExtractionRule.Token.Name == filter.Token && v.ValueNumber == value),
            "boolean" when bool.TryParse(filter.Value, out var value)
                => e => e.ExtractedValues.Any(v =>
                    v.ExtractionRule.Token.Name == filter.Token && v.ValueBoolean == value),
            "date" when DateOnly.TryParse(filter.Value, CultureInfo.InvariantCulture, out var value)
                => e => e.ExtractedValues.Any(v =>
                    v.ExtractionRule.Token.Name == filter.Token && v.ValueDate == value),
            "timestamp" when DateTimeOffset.TryParse(
                filter.Value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var value)
                => e => e.ExtractedValues.Any(v =>
                    v.ExtractionRule.Token.Name == filter.Token && v.ValueTimestamp == value),
            "uuid" when Guid.TryParse(filter.Value, out var value)
                => e => e.ExtractedValues.Any(v =>
                    v.ExtractionRule.Token.Name == filter.Token && v.ValueUuid == value),
            "json" => throw new ArgumentException("JSON token equality queries are not currently supported."),
            _ => throw new ArgumentException(
                $"Value '{filter.Value}' is not valid for token '{filter.Token}' of type '{valueType}'."),
        };
    }

    private static IQueryable<EventEntity> ApplySorting(
        IQueryable<EventEntity> query,
        EventSortBy sortBy,
        SortOrder sortOrder)
    {
        return (sortBy, sortOrder) switch
        {
            (EventSortBy.Title, SortOrder.Ascending) => query.OrderBy(x => x.Title).ThenBy(x => x.Id),
            (EventSortBy.Title, SortOrder.Descending) => query.OrderByDescending(x => x.Title).ThenByDescending(x => x.Id),
            (EventSortBy.CountyParishHolding, SortOrder.Ascending) => query.OrderBy(x => x.CountyParishHolding).ThenBy(x => x.Id),
            (EventSortBy.CountyParishHolding, SortOrder.Descending) => query.OrderByDescending(x => x.CountyParishHolding).ThenByDescending(x => x.Id),
            (EventSortBy.CreatedBy, SortOrder.Ascending) => query.OrderBy(x => x.CreatedBy).ThenBy(x => x.Id),
            (EventSortBy.CreatedBy, SortOrder.Descending) => query.OrderByDescending(x => x.CreatedBy).ThenByDescending(x => x.Id),
            (EventSortBy.CreatedAt, SortOrder.Ascending) => query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id),
            _ => query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id),
        };
    }

    private static Expression<Func<EventEntity, bool>> OrElse(
        Expression<Func<EventEntity, bool>> left,
        Expression<Func<EventEntity, bool>> right)
    {
        var parameter = left.Parameters[0];
        var rightBody = new ReplaceParameterVisitor(right.Parameters[0], parameter).Visit(right.Body)!;
        return Expression.Lambda<Func<EventEntity, bool>>(Expression.OrElse(left.Body, rightBody), parameter);
    }

    private async Task<IReadOnlyCollection<ResolvedFilter>> ResolveFiltersAsync(
        IReadOnlyCollection<EventTokenFilter> filters,
        CancellationToken cancellationToken)
    {
        var tokenNames = filters.Select(x => x.Token).Distinct().ToList();
        var tokenTypes = await context.Set<EventExtractionRule>()
            .AsNoTracking()
            .Where(x => tokenNames.Contains(x.Token.Name))
            .Select(x => new { x.Token.Name, x.ValueType })
            .Distinct()
            .ToListAsync(cancellationToken);

        return filters.Select(filter =>
        {
            var valueTypes = tokenTypes
                .Where(x => x.Name == filter.Token)
                .Select(x => x.ValueType)
                .Distinct()
                .ToList();

            if (valueTypes.Count == 0)
            {
                throw new ArgumentException($"Unknown event query token '{filter.Token}'.");
            }

            if (valueTypes.Count > 1)
            {
                throw new ArgumentException($"Event query token '{filter.Token}' has inconsistent value types.");
            }

            return new ResolvedFilter(filter, valueTypes[0]);
        }).ToList();
    }

    private sealed record ResolvedFilter(EventTokenFilter Filter, string ValueType);

    private sealed class ReplaceParameterVisitor(ParameterExpression source, ParameterExpression target)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == source ? target : base.VisitParameter(node);
        }
    }
}

// <copyright file="QueryEndpoints.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Api.Endpoints.Public;

using System.Net.Mime;
using Defra.Lis.EventLogging.Api.Filters;
using Defra.Lis.EventLogging.Models.Requests.Logging;
using Defra.Lis.EventLogging.Models.Responses.Logging;
using Defra.Lis.EventLogging.Services;
using Microsoft.AspNetCore.Mvc;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class QueryEndpoints
{
    public static void UseQueryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(RouteNames.Query, QueryEventsRoute)
            .WithName("QueryEvents")
            .WithTags(nameof(RouteNames.Query))
            .AddEndpointFilter<ValidationFilter<QueryEvents>>()
            .Produces<PagedEventResult>(StatusCodes.Status200OK, MediaTypeNames.Application.Json);

        app.MapGet($"{RouteNames.Events}/{{logId:guid}}", GetEventRoute)
            .WithName("GetEvent")
            .WithTags(nameof(RouteNames.Events))
            .Produces<EventResult>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet($"{RouteNames.Events}/short-id/{{shortId}}", GetEventByShortIdRoute)
            .WithName("GetEventByShortId")
            .WithTags(nameof(RouteNames.Events))
            .Produces<EventResult>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapGet($"{RouteNames.Events}/{{logId:guid}}/artefacts/{{artefactId:guid}}", GetArtefactRoute)
            .WithName("GetEventArtefact")
            .WithTags(nameof(RouteNames.Events))
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> QueryEventsRoute(
        [FromBody] QueryEvents request,
        [FromServices] IEventQueryService service,
        CancellationToken cancellationToken)
    {
        var result = await service.QueryEventsAsync(request, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetEventRoute(
        Guid logId,
        [FromServices] IEventQueryService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetEventAsync(logId, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> GetEventByShortIdRoute(
        string shortId,
        [FromServices] IEventQueryService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetEventByShortIdAsync(shortId, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> GetArtefactRoute(
        Guid logId,
        Guid artefactId,
        [FromServices] IArtefactRetrievalService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetArtefactAsync(logId, artefactId, cancellationToken);

        return result is null
            ? Results.NotFound()
            : Results.Stream(
                result.Content,
                result.MimeType,
                result.Filename,
                enableRangeProcessing: false);
    }
}

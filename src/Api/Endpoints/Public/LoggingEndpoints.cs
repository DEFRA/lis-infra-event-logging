// <copyright file="LoggingEndpoints.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Api.Endpoints.Public;

using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Defra.Lis.EventLogging.Api.Filters;
using Defra.Lis.EventLogging.Api.Middleware.Headers;
using Defra.Lis.EventLogging.Api.Models;
using Defra.Lis.EventLogging.Models.Requests.Logging;
using Defra.Lis.EventLogging.Models.Responses.Logging;
using Defra.Lis.EventLogging.Services;
using Defra.Lis.EventLogging.Services.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class LoggingEndpoints
{
    public static void UseLoggingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(RouteNames.Log, SubmitEventRoute)
            .WithName("SubmitEvent")
            .WithTags(nameof(RouteNames.Log))
            .AddEndpointFilter<SubmissionHeadersFilter>()
            .AddEndpointFilter<ValidationFilter<PostEvent>>()
            .Produces<EventSubmissionResult>(StatusCodes.Status202Accepted, MediaTypeNames.Application.Json);

        app.MapPost($"{RouteNames.Log}/with-artefact", SubmitEventWithArtefactRoute)
            .WithName("SubmitEventWithArtefact")
            .WithTags(nameof(RouteNames.Log))
            .AddEndpointFilter<SubmissionHeadersFilter>()
            .DisableAntiforgery()
            .Accepts<PostEventWithArtefactForm>("multipart/form-data")
            .Produces<EventSubmissionResult>(StatusCodes.Status202Accepted, MediaTypeNames.Application.Json);

        app.MapPost($"{RouteNames.Log}/{{logId:guid}}/artefacts", SubmitArtefactRoute)
            .WithName("SubmitArtefact")
            .WithTags(nameof(RouteNames.Log))
            .AddEndpointFilter<SubmissionHeadersFilter>()
            .DisableAntiforgery()
            .Accepts<PostArtefactForm>("multipart/form-data")
            .Produces<EventSubmissionResult>(StatusCodes.Status202Accepted, MediaTypeNames.Application.Json);

        app.MapGet($"{RouteNames.Submissions}/{{submissionId:guid}}", GetSubmissionStatusRoute)
            .WithName("GetEventSubmissionStatus")
            .WithTags(nameof(RouteNames.Submissions))
            .Produces<EventSubmissionStatusResult>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> SubmitEventRoute(
        [FromBody] PostEvent request,
        HttpContext httpContext,
        [FromServices] IEventLoggingService service,
        CancellationToken cancellationToken)
    {
        var result = await service.SubmitEventAsync(
            request,
            CreateSubmissionContext(httpContext),
            cancellationToken);

        return SubmissionAccepted(result);
    }

    private static async Task<IResult> SubmitEventWithArtefactRoute(
        [FromForm] PostEventWithArtefactForm form,
        HttpContext httpContext,
        [FromServices] IEventLoggingService service,
        [FromServices] IValidator<PostEventWithArtefact> validator,
        CancellationToken cancellationToken)
    {
        var request = MapRequest(form);
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Results.UnprocessableEntity(validation.ToDictionary());
        }

        var result = await service.SubmitEventWithArtefactAsync(
            request,
            CreateSubmissionContext(httpContext),
            cancellationToken);

        return SubmissionAccepted(result);
    }

    private static async Task<IResult> SubmitArtefactRoute(
        Guid logId,
        [FromForm] PostArtefactForm form,
        HttpContext httpContext,
        [FromServices] IEventLoggingService service,
        [FromServices] IValidator<PostArtefact> validator,
        CancellationToken cancellationToken)
    {
        var request = MapRequest(form.Artefact);
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Results.UnprocessableEntity(validation.ToDictionary());
        }

        var result = await service.SubmitArtefactAsync(
            logId,
            request,
            CreateSubmissionContext(httpContext),
            cancellationToken);

        return SubmissionAccepted(result);
    }

    private static async Task<IResult> GetSubmissionStatusRoute(
        Guid submissionId,
        [FromServices] IEventLoggingService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetSubmissionStatusAsync(submissionId, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static IResult SubmissionAccepted(EventSubmissionResult result)
    {
        return Results.Accepted($"/{RouteNames.Submissions}/{result.SubmissionId}", result);
    }

    private static SubmissionContext CreateSubmissionContext(HttpContext context)
    {
        var apiKey = context.Request.Headers[RequestHeaderNames.ApiKey].ToString();

        return new SubmissionContext()
        {
            ClientId = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(apiKey))),
            IdempotencyKey = context.Request.Headers[RequestHeaderNames.IdempotencyKey].ToString(),
            CorrelationId = Guid.Parse(context.Request.Headers[RequestHeaderNames.CorrelationId].ToString()),
        };
    }

    private static PostEventWithArtefact MapRequest(PostEventWithArtefactForm form)
    {
        return new PostEventWithArtefact()
        {
            Event = new PostEvent()
            {
                CountyParishHolding = form.CountyParishHolding,
                CreatedAt = form.CreatedAt,
                Title = form.Title,
                Data = string.IsNullOrWhiteSpace(form.Data) ? null : JsonDocument.Parse(form.Data),
                CreatedBy = form.CreatedBy,
                Taxonomy = form.Taxonomy,
                SubTaxonomy = form.SubTaxonomy,
                Species = form.Species,
            },
            Artefact = MapRequest(form.Artefact),
        };
    }

    private static PostArtefact MapRequest(IFormFile? file)
    {
        return new PostArtefact()
        {
            Content = file?.OpenReadStream() ?? Stream.Null,
            MimeType = file?.ContentType ?? string.Empty,
            OriginalFilename = file?.FileName ?? string.Empty,
            Size = file?.Length ?? 0,
        };
    }
}

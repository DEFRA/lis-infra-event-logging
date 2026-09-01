// <copyright file="HealthEndpoints.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Api.Endpoints.Health;

using Defra.Lis.EventLogging.Api.MetaData;
using Defra.Lis.EventLogging.Models.Responses.Health;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class HealthEndpoints
{
    public static void UseHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(RouteNames.Health, CalculateHealthRoute)
            .WithName(OpenApiMetadata.Get.Name)
            .WithTags(OpenApiMetadata.Tag)
            .WithSummary(OpenApiMetadata.Get.Summary)
            .WithDescription(OpenApiMetadata.Get.Description)
            .WithMetadata(new IgnoreCorrelationIdCheck())
            .WithMetadata(new IgnoreApiKeyCheck());
    }

    private static Task<IResult> CalculateHealthRoute()
    {
        return Task.FromResult(Results.Ok(new HealthStatus() { Status = "ok" }));
    }
}

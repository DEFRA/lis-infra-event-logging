// <copyright file="OpenApiMetadata.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Api.Endpoints.Health;

public static class OpenApiMetadata
{
    public const string Tag = nameof(RouteNames.Health);

    public static class Get
    {
        public const string Name = "GetHealth";
        public const string Summary = "Get health status";
        public const string Description = "Retrieves the health status of the API and its dependencies";
    }
}

// <copyright file="OpenApiMetadata.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Api.Endpoints.Public;

public static class OpenApiMetadata
{
    public static class PostLogEventRoute
    {
        public const string Name = "PostLogEventRoute";
        public const string Summary = "Add a new event";
        public const string Description = "Add a new event";
    }

    public static class PostAddArtefactToEventRoute
    {
        public const string Name = "PostAddArtefactToEventRoute";
        public const string Summary = "Add an artefact to an event.";
        public const string Description = "Add an artefact to an event.";
    }
}

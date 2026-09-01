// <copyright file="MiddlewareBase.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Api.Middleware.Base;

using System.Text.Json;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public abstract class MiddlewareBase : IMiddleware
{
    public abstract Task InvokeAsync(HttpContext context, RequestDelegate next);

    protected static async Task WriteJsonErrorAsync(
        HttpContext context,
        int statusCode,
        string code,
        string message,
        object? details = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = new
        {
            error = new
            {
                code,
                message,
                traceId = context.TraceIdentifier,
                path = context.Request.Path.Value,
                details,
            },
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}

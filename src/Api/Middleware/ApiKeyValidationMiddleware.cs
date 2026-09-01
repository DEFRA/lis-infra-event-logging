// <copyright file="ApiKeyValidationMiddleware.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Api.Middleware;

using Defra.Lis.EventLogging.Api.MetaData;
using Defra.Lis.EventLogging.Api.Middleware.Base;
using Defra.Lis.EventLogging.Api.Middleware.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class ApiKeyValidationMiddleware(string apiKey, ILogger<ApiKeyValidationMiddleware> logger)
    : MiddlewareBase
{
    public override async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKey);

        var endpoint = context.GetEndpoint();

        if (endpoint == null)
        {
            await next(context);
            return;
        }

        var ignoreApiKeyCheck = endpoint.Metadata.GetMetadata<IgnoreApiKeyCheck>() is not null;

        if (ignoreApiKeyCheck)
        {
            await next(context);
            return;
        }

        try
        {
            var headers = context.Request.Headers;

            headers.TryGetValue(RequestHeaderNames.ApiKeyHeader, out var key);

            if (string.IsNullOrWhiteSpace(key))
            {
                await WriteJsonErrorAsync(
                    context,
                    statusCode: StatusCodes.Status400BadRequest,
                    code: "missing_header",
                    message: $"Header {RequestHeaderNames.ApiKeyHeader} is required.",
                    details: new { header = $"{RequestHeaderNames.ApiKeyHeader}" });
                return;
            }

            if (!string.Equals(key, apiKey, StringComparison.Ordinal))
            {
                await WriteJsonErrorAsync(
                    context,
                    statusCode: StatusCodes.Status400BadRequest,
                    code: "invalid_api_key",
                    message: $"Header {RequestHeaderNames.ApiKeyHeader} is not valid.",
                    details: new { header = $"{RequestHeaderNames.ApiKeyHeader}" });
                return;
            }

            await next(context);
        }
        catch (Exception ex)
        {
            LogErrorInMiddleware(logger, nameof(ApiKeyValidationMiddleware), ex);

            throw;
        }
    }
}

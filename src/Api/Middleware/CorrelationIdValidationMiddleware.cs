// <copyright file="CorrelationIdValidationMiddleware.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Api.Middleware;

using Defra.Lis.EventLogging.Api.MetaData;
using Defra.Lis.EventLogging.Api.Middleware.Base;
using Defra.Lis.EventLogging.Api.Middleware.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class CorrelationIdValidationMiddleware(ILogger<CorrelationIdValidationMiddleware> logger)
    : MiddlewareBase
{
    public override async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();

        if (endpoint == null)
        {
            await next(context);
            return;
        }

        var ignoreCorrelationIdCheck = endpoint.Metadata.GetMetadata<IgnoreCorrelationIdCheck>() is not null;

        if (ignoreCorrelationIdCheck)
        {
            await next(context);
            return;
        }

        try
        {
            var headers = context.Request.Headers;

            var correlationId = SanitizeHeaderValue(headers.TryGetValue(RequestHeaderNames.CorrelationId, out var tid)
                ? tid.ToString()
                : null);

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                await WriteJsonErrorAsync(
                    context,
                    statusCode: StatusCodes.Status400BadRequest,
                    code: "missing_header",
                    message: $"Header {RequestHeaderNames.CorrelationId} is required.",
                    details: new { header = $"{RequestHeaderNames.CorrelationId}" });
                return;
            }

            await next(context);
        }
        catch (Exception ex)
        {
            LogErrorInMiddleware(logger, nameof(CorrelationIdValidationMiddleware), ex);

            throw;
        }
    }

    private static string? SanitizeHeaderValue(string? value)
    {
        if (value is null)
        {
            return null;
        }

        var trimmedValue = value.Trim().Trim('\"', '\'');

        return trimmedValue.Length == 0 ? null : trimmedValue;
    }
}

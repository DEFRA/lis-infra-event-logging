// <copyright file="SubmissionHeadersFilter.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Api.Filters;

using Defra.Lis.EventLogging.Api.Middleware.Headers;

public class SubmissionHeadersFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var idempotencyKey = context.HttpContext.Request.Headers[RequestHeaderNames.IdempotencyKey].ToString();

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return Results.BadRequest(new
            {
                Code = "missing_header",
                Message = $"Header {RequestHeaderNames.IdempotencyKey} is required.",
            });
        }

        if (idempotencyKey.Length > 255)
        {
            return Results.BadRequest(new
            {
                Code = "invalid_header",
                Message = $"Header {RequestHeaderNames.IdempotencyKey} must not exceed 255 characters.",
            });
        }

        return await next(context);
    }
}

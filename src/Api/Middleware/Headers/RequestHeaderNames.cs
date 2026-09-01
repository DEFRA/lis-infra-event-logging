// <copyright file="RequestHeaderNames.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Api.Middleware.Headers;

public static class RequestHeaderNames
{
    public const string CorrelationId = "x-cdp-request-id";
    public const string ApiKeyHeader = "x-api-key";
    public const string IdempotencyKey = "idempotency-key";
}

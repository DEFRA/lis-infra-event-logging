// <copyright file="RequestHeaderNamesTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Api.Tests.Middleware.Headers;

using Defra.Lis.EventLogging.Api.Middleware.Headers;

public class RequestHeaderNamesTests
{
    [Fact]
    public void HeaderNames_Should_Have_Correct_Values()
    {
        RequestHeaderNames.CorrelationId.ShouldBe("x-cdp-request-id");
        RequestHeaderNames.ApiKey.ShouldBe("x-api-key");
        RequestHeaderNames.IdempotencyKey.ShouldBe("idempotency-key");
    }
}

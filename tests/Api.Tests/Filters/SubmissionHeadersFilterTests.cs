// <copyright file="SubmissionHeadersFilterTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Api.Tests.Filters;

using Defra.Lis.EventLogging.Api.Filters;
using Defra.Lis.EventLogging.Api.Middleware.Headers;
using Microsoft.AspNetCore.Http;

public class SubmissionHeadersFilterTests
{
    private readonly SubmissionHeadersFilter filter = new();

    [Fact]
    public async Task Should_Reject_A_Missing_Idempotency_Key()
    {
        var context = EndpointFilterInvocationContext.Create(new DefaultHttpContext());
        var nextCalled = false;

        var result = await filter.InvokeAsync(context, _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        });

        ((IStatusCodeHttpResult)result!).StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        nextCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Reject_An_Overlong_Idempotency_Key()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[RequestHeaderNames.IdempotencyKey] = new string('a', 256);
        var context = EndpointFilterInvocationContext.Create(httpContext);

        var result = await filter.InvokeAsync(
            context,
            _ => ValueTask.FromResult<object?>(Results.Ok()));

        ((IStatusCodeHttpResult)result!).StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Should_Continue_When_Idempotency_Key_Is_Valid()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[RequestHeaderNames.IdempotencyKey] = "submission-123";
        var context = EndpointFilterInvocationContext.Create(httpContext);
        var expected = new object();

        var result = await filter.InvokeAsync(
            context,
            _ => ValueTask.FromResult<object?>(expected));

        result.ShouldBeSameAs(expected);
    }
}

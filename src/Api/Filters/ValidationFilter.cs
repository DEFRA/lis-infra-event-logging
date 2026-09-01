// <copyright file="ValidationFilter.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Api.Filters;

using FluentValidation;
using Microsoft.AspNetCore.Http;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class ValidationFilter<T>(IValidator<T> validator)
    : IEndpointFilter
    where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var arg = context.Arguments.OfType<T>().FirstOrDefault();

        if (arg == null)
        {
            return Results.BadRequest("Invalid request.");
        }

        var validationResult = await validator.ValidateAsync(arg, context.HttpContext.RequestAborted);

        if (!validationResult.IsValid)
        {
            return Results.UnprocessableEntity(validationResult.ToDictionary());
        }

        return await next(context);
    }
}

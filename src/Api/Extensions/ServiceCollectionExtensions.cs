// <copyright file="ServiceCollectionExtensions.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Api.Extensions;

using Defra.Lis.EventLogging.Api.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static string? ApiKey { get; private set; }

    public static IServiceCollection AddRequests(this IServiceCollection services, IConfigurationRoot config)
    {
        ApiKey = config.GetValue<string>("ServiceApiKey");

        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            throw new ArgumentException("DefraIdentityApiKey configuration value is missing or empty");
        }

        services.AddTransient<ApiKeyValidationMiddleware>(sp =>
            new ApiKeyValidationMiddleware(ApiKey, sp.GetRequiredService<ILogger<ApiKeyValidationMiddleware>>()));

        services.AddTransient<CorrelationIdValidationMiddleware>();

        return services;
    }

    public static void UseRequests(this WebApplication app)
    {
        app.UseMiddleware<ApiKeyValidationMiddleware>();
        app.UseMiddleware<CorrelationIdValidationMiddleware>();
    }
}

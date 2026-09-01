// <copyright file="Program.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Api;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Defra.Lis.EventLogging.Api.Endpoints.Health;
using Defra.Lis.EventLogging.Api.Endpoints.Public;
using Defra.Lis.EventLogging.Api.Exceptions;
using Defra.Lis.EventLogging.Api.Extensions;
using Defra.Lis.EventLogging.Api.Utils.Logging;
using Defra.Lis.EventLogging.Database;
using Defra.Lis.EventLogging.Models;
using Defra.Lis.EventLogging.Repositories;
using Defra.Lis.EventLogging.Services;
using Defra.Lis.EventLogging.Worker;
using Serilog;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static async Task Main(string[] args)
    {
        var app = CreateWebApplication(args);
        await app.RunAsync();
    }

    private static WebApplication CreateWebApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var configuration = builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile(
                $"appsettings.{builder.Environment.EnvironmentName}.json",
                optional: true,
                reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        ConfigureBuilder(builder, configuration.Build());

        var app = builder.Build();
        return SetupApplication(app);
    }

    private static void ConfigureBuilder(
        WebApplicationBuilder builder,
        IConfigurationRoot configuration)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHealthChecks();
        builder.Host.UseSerilog(CdpLogging.Configuration);
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<ApiExceptionHandler>();

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
            options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
        });

        builder.Services
            .AddEventLoggingDatabaseConfigurations()
            .AddRepositories(configuration)
            .AddRequests(configuration)
            .AddValidators()
            .AddServices(configuration)
            .AddEventSubmissionWorkers(configuration);
    }

    [ExcludeFromCodeCoverage]
    private static WebApplication SetupApplication(WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.UseExceptionHandler();
        app.UseRouting();
        app.UseRequests();
        app.UseHealthEndpoints();
        app.UseLoggingEndpoints();
        app.UseQueryEndpoints();

        return app;
    }
}

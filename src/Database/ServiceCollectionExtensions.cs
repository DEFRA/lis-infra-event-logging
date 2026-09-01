// <copyright file="ServiceCollectionExtensions.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database;

using Defra.Database.Postgres;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventLoggingDatabaseConfigurations(this IServiceCollection services)
    {
        services.ConfigureDbContext<PostgresDbContext>((_, options) =>
        {
            options.ReplaceService<IModelCustomizer, EventLoggingModelCustomizer>();
        });

        services.ConfigureDbContext<ReadOnlyPostgresDbContext>((_, options) =>
        {
            options.ReplaceService<IModelCustomizer, EventLoggingModelCustomizer>();
        });

        return services;
    }
}

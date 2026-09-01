// <copyright file="EventLoggingModelCustomizer.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database;

using Defra.Database.Postgres;
using Defra.Lis.EventLogging.Database.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;

public class EventLoggingModelCustomizer(ModelCustomizerDependencies dependencies)
    : ModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        if (context is PostgresDbContext or ReadOnlyPostgresDbContext)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventConfiguration).Assembly);
        }
    }
}

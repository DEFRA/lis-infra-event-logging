// <copyright file="Program.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Worker;

using Amazon.Extensions.NETCore.Setup;
using Amazon.SQS;
using Defra.Lis.EventLogging.Database;
using Defra.Lis.EventLogging.Repositories;
using Defra.Lis.EventLogging.Services;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        var configuration = (IConfigurationRoot)builder.Configuration;

        builder.Services
            .AddEventLoggingDatabaseConfigurations()
            .AddRepositories(configuration)
            .AddServices(configuration);
        builder.Services.AddDefaultAWSOptions(configuration.GetAWSOptions());
        builder.Services.AddAWSService<IAmazonSQS>();
        builder.Services.Configure<QueueOptions>(configuration.GetSection(QueueOptions.SectionName));
        builder.Services.AddHostedService<OutboxPublisherService>();
        builder.Services.AddHostedService<EventSubmissionConsumerService>();

        await builder.Build().RunAsync();
    }
}

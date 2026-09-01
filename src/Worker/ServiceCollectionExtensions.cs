// <copyright file="ServiceCollectionExtensions.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Worker;

using System.Diagnostics.CodeAnalysis;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventSubmissionWorkers(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var queueSection = configuration.GetSection(QueueOptions.SectionName);
        services.Configure<QueueOptions>(queueSection);

        if (string.IsNullOrWhiteSpace(queueSection[nameof(QueueOptions.QueueUrl)]))
        {
            return services;
        }

        services.AddDefaultAWSOptions(configuration.GetAWSOptions());
        services.AddAWSService<IAmazonSQS>();
        services.AddHostedService<OutboxPublisherService>();
        services.AddHostedService<EventSubmissionConsumerService>();
        services.AddHostedService<EventSubmissionCleanupService>();

        return services;
    }
}

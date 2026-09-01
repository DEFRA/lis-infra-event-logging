// <copyright file="ServiceCollectionExtensions.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Repositories;

using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Defra.Lis.EventLogging.Repositories.Artefacts;
using Defra.Lis.EventLogging.Repositories.Events;
using Defra.Lis.EventLogging.Repositories.Submissions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddRepositories(IConfigurationRoot configuration)
        {
            services.AddDefaultAWSOptions(configuration.GetAWSOptions());
            services.AddAWSService<IAmazonS3>();
            services.Configure<ArtefactStorageOptions>(
                configuration.GetSection(ArtefactStorageOptions.SectionName));
            services.AddTransient<IArtefactRepository, ArtefactRepository>();
            services.AddTransient<IArtefactStore, S3ArtefactStore>();
            services.AddTransient<IEventQueryRepository, EventQueryRepository>();
            services.AddTransient<IEventSubmissionRepository, EventSubmissionRepository>();
            services.AddTransient<IEventSubmissionProcessingRepository, EventSubmissionProcessingRepository>();

            return services;
        }
    }
}

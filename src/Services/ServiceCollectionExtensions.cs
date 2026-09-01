// <copyright file="ServiceCollectionExtensions.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services;

using Defra.Lis.EventLogging.Services.Thumbnails;
using Defra.Livestock.Sdk.Api.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddServices(IConfigurationRoot config)
        {
            services.AddStrategyFramework();
            services.AddTransient<IArtefactRetrievalService, ArtefactRetrievalService>();
            services.AddTransient<IArtefactThumbnailProcessor, ArtefactThumbnailProcessor>();
            services.AddTransient<IEventQueryService, EventQueryService>();
            services.AddTransient<IEventLoggingService, EventLoggingService>();
            services.AddTransient<IEventSubmissionProcessor, EventSubmissionProcessor>();
            services.AddTransient<IThumbnailGenerator, ImageThumbnailGenerator>();
            services.AddTransient<IThumbnailGenerator, PdfThumbnailGenerator>();
            services.AddTransient<IThumbnailService, ThumbnailService>();

            return services;
        }
    }
}

// <copyright file="ServiceCollectionExtensions.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models;

using Defra.Lis.EventLogging.Models.Requests.Logging;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddValidators()
        {
            services.AddValidatorsFromAssemblyContaining<PostEventValidator>();

            return services;
        }
    }
}

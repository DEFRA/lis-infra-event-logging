// <copyright file="IEventSubmissionProcessor.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services;

using Defra.Lis.EventLogging.Models.Messages;

public interface IEventSubmissionProcessor
{
    Task ProcessAsync(EventSubmissionMessage message, CancellationToken cancellationToken = default);
}

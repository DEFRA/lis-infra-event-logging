// <copyright file="IEventLoggingService.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Services;

using Defra.Lis.EventLogging.Models.Requests.Logging;
using Defra.Lis.EventLogging.Models.Responses.Logging;
using Defra.Lis.EventLogging.Services.Models;

public interface IEventLoggingService
{
    Task<EventAcceptedResult> SubmitEventAsync(
        PostEvent request,
        SubmissionContext context,
        CancellationToken cancellationToken = default);

    Task<EventAcceptedResult> SubmitEventWithArtefactAsync(
        PostEventWithArtefact request,
        SubmissionContext context,
        CancellationToken cancellationToken = default);

    Task<EventAcceptedResult> SubmitArtefactAsync(
        Guid logId,
        PostArtefact request,
        SubmissionContext context,
        CancellationToken cancellationToken = default);
}

// <copyright file="IEventSubmissionProcessingRepository.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Repositories.Submissions;

using Defra.Lis.EventLogging.Database.Entities;
using Defra.Lis.EventLogging.Models.Messages;

public interface IEventSubmissionProcessingRepository
{
    Task<IReadOnlyList<OutboxMessage>> GetUnpublishedAsync(
        int limit,
        CancellationToken cancellationToken = default);

    Task MarkPublishedAsync(Guid messageId, CancellationToken cancellationToken = default);

    Task MarkPublishFailedAsync(
        Guid messageId,
        string error,
        CancellationToken cancellationToken = default);

    Task<bool> CompleteAsync(
        EventSubmissionMessage message,
        CancellationToken cancellationToken = default);

    Task MarkSubmissionFailedAsync(
        Guid submissionId,
        string failureCode,
        CancellationToken cancellationToken = default);

    Task<int> DeleteTerminalSubmissionsAsync(
        DateTimeOffset olderThan,
        int limit,
        CancellationToken cancellationToken = default);
}

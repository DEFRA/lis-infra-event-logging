// <copyright file="EventSubmission.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database.Entities;

using Defra.Lis.EventLogging.Database.Domain;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class EventSubmission
{
    public Guid Id { get; set; }

    public SubmissionType Type { get; set; }

    public SubmissionStatus Status { get; set; }

    public Guid LogId { get; set; }

    public Guid? ArtefactId { get; set; }

    public string ShortId { get; set; } = null!;

    public string ClientId { get; set; } = null!;

    public string IdempotencyKey { get; set; } = null!;

    public string RequestFingerprint { get; set; } = null!;

    public Guid CorrelationId { get; set; }

    public string? PendingS3Key { get; set; }

    public string? OriginalFilename { get; set; }

    public string? MimeType { get; set; }

    public string? FailureCode { get; set; }

    public DateTimeOffset SubmittedAt { get; set; }

    public DateTimeOffset? ProcessingStartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<OutboxMessage> OutboxMessages { get; set; } = [];
}

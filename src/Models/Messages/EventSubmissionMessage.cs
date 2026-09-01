// <copyright file="EventSubmissionMessage.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Messages;

using System.Text.Json;

public record EventSubmissionMessage
{
    public required string MessageType { get; init; }

    public int SchemaVersion { get; init; } = 1;

    public required Guid SubmissionId { get; init; }

    public required Guid LogId { get; init; }

    public Guid? ArtefactId { get; init; }

    public required string ShortId { get; init; }

    public string? CountyParishHolding { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public string? Title { get; init; }

    public JsonDocument? Data { get; init; }

    public string? CreatedBy { get; init; }

    public Guid? SubTaxonomyId { get; init; }

    public string? PendingS3Key { get; init; }

    public string? OriginalFilename { get; init; }

    public string? MimeType { get; init; }
}

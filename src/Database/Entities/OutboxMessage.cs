// <copyright file="OutboxMessage.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database.Entities;

using System.Text.Json;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class OutboxMessage
{
    public Guid Id { get; set; }

    public Guid SubmissionId { get; set; }

    public string MessageType { get; set; } = null!;

    public int SchemaVersion { get; set; }

    public JsonDocument Payload { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }

    public int AttemptCount { get; set; }

    public string? LastError { get; set; }

    public EventSubmission Submission { get; set; } = null!;
}

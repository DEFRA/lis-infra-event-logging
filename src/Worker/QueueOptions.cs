// <copyright file="QueueOptions.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Worker;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class QueueOptions
{
    public const string SectionName = "EventSubmissionQueue";

    public string QueueUrl { get; set; } = string.Empty;

    public int WaitTimeSeconds { get; set; } = 20;

    public int VisibilityTimeoutSeconds { get; set; } = 120;

    public int OutboxBatchSize { get; set; } = 10;

    public int OutboxPollIntervalSeconds { get; set; } = 2;
}

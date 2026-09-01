// <copyright file="EventArtefact.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Database.Entities;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class EventArtefact
{
    public Guid Id { get; set; }

    public Guid EventId { get; set; }

    public string MimeType { get; set; } = null!;

    public string OriginalFilename { get; set; } = null!;

    public string S3Path { get; set; } = null!;

    public byte[]? Thumbnail { get; set; }

    public string? ThumbnailMimeType { get; set; }

    public int? ThumbnailWidth { get; set; }

    public int? ThumbnailHeight { get; set; }

    public ThumbnailStatus ThumbnailStatus { get; set; }

    public string? ThumbnailFailureCode { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Event Event { get; set; } = null!;
}

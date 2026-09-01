// <copyright file="PostEventWithArtefactForm.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Api.Models;

public class PostEventWithArtefactForm
{
    public string? CountyParishHolding { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string? Title { get; set; }

    public string? Data { get; set; }

    public string? CreatedBy { get; set; }

    public string? Taxonomy { get; set; }

    public string? SubTaxonomy { get; set; }

    public string? Species { get; set; }

    public IFormFile? Artefact { get; set; }
}

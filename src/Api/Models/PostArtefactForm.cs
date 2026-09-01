// <copyright file="PostArtefactForm.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Api.Models;

public class PostArtefactForm
{
    public IFormFile? Artefact { get; set; }
}

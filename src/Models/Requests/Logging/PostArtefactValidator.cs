// <copyright file="PostArtefactValidator.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Requests.Logging;

using FluentValidation;

public class PostArtefactValidator : AbstractValidator<PostArtefact>
{
    public PostArtefactValidator()
    {
        RuleFor(x => x.Content).NotNull().Must(x => x?.CanRead == true);
        RuleFor(x => x.MimeType).NotEmpty().MaximumLength(255);
        RuleFor(x => x.OriginalFilename).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Size).GreaterThan(0);
    }
}

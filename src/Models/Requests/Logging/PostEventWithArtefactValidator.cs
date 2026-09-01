// <copyright file="PostEventWithArtefactValidator.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Requests.Logging;

using FluentValidation;

public class PostEventWithArtefactValidator
    : AbstractValidator<PostEventWithArtefact>
{
    public PostEventWithArtefactValidator()
    {
        RuleFor(x => x.Event).NotNull().SetValidator(new PostEventValidator());
        RuleFor(x => x.Artefact).NotNull().SetValidator(new PostArtefactValidator());
    }
}

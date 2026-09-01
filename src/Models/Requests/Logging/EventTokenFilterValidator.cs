// <copyright file="EventTokenFilterValidator.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Requests.Logging;

using FluentValidation;

public class EventTokenFilterValidator : AbstractValidator<EventTokenFilter>
{
    public EventTokenFilterValidator()
    {
        RuleFor(x => x.Token).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Value).NotEmpty().MaximumLength(1024);
    }
}

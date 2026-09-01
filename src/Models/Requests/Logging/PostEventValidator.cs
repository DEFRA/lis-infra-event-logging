// <copyright file="PostEventValidator.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Requests.Logging;

using FluentValidation;

public class PostEventValidator
    : AbstractValidator<PostEvent>
{
    public PostEventValidator()
    {
        const string cphConstraint = @"^\d{2}/\d{3}/\d{4}$";

        RuleFor(x => x.CountyParishHolding).NotEmpty().MaximumLength(11).Matches(cphConstraint);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Species).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Taxonomy).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SubTaxonomy).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CreatedBy).NotEmpty().MaximumLength(50);
    }
}

// <copyright file="QueryEventsValidator.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Requests.Logging;

using FluentValidation;

public class QueryEventsValidator : AbstractValidator<QueryEvents>
{
    public QueryEventsValidator()
    {
        const string cphConstraint = @"^\d{2}/\d{3}/\d{4}$";

        RuleFor(x => x.CountyParishHolding)
            .MaximumLength(11)
            .Matches(cphConstraint)
            .When(x => !string.IsNullOrWhiteSpace(x.CountyParishHolding));
        RuleFor(x => x.CountyParishHolding)
            .Must(x => x is null || !string.IsNullOrWhiteSpace(x))
            .WithMessage("County parish holding must not be empty when supplied.");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 25);
        RuleForEach(x => x.Filters).SetValidator(new EventTokenFilterValidator());
        RuleFor(x => x.Filters)
            .Must(x => x.Distinct().Count() == x.Count)
            .WithMessage("Duplicate token/value filters are not allowed.");
    }
}

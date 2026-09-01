// <copyright file="QueryEventsValidatorTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Tests.Requests;

using Defra.Lis.EventLogging.Models.Requests.Logging;
using FluentValidation.TestHelper;

public class QueryEventsValidatorTests
{
    private readonly QueryEventsValidator validator = new();

    [Fact]
    public void Should_Allow_Query_Without_Cph_Or_Filters()
    {
        var result = this.validator.TestValidate(new QueryEvents());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("12/345/6789")]
    [InlineData(null)]
    public void Should_Allow_Valid_Optional_Cph(string? cph)
    {
        var result = this.validator.TestValidate(new QueryEvents() { CountyParishHolding = cph, });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("12/34/5678")]
    public void Should_Reject_Invalid_Cph(string cph)
    {
        var result = this.validator.TestValidate(new QueryEvents() { CountyParishHolding = cph, });

        result.ShouldHaveValidationErrorFor(x => x.CountyParishHolding);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(26)]
    public void Should_Reject_Page_Size_Outside_One_To_TwentyFive(int pageSize)
    {
        var result = this.validator.TestValidate(new QueryEvents() { PageSize = pageSize, });

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Should_Reject_Page_Below_One()
    {
        var result = this.validator.TestValidate(new QueryEvents() { Page = 0, });

        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void Should_Allow_Multiple_Different_Filters()
    {
        var result = this.validator.TestValidate(new QueryEvents()
        {
            Filters =
            [
                new EventTokenFilter() { Token = "ear_tag", Value = "UK1", },
                new EventTokenFilter() { Token = "submission_ref", Value = "SUB1", },
            ],
        });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Reject_Duplicate_Filters()
    {
        var filter = new EventTokenFilter() { Token = "ear_tag", Value = "UK1", };
        var result = this.validator.TestValidate(new QueryEvents() { Filters = [filter, filter], });

        result.ShouldHaveValidationErrorFor(x => x.Filters);
    }
}

// <copyright file="PostEventValidatorTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Tests.Requests;

using Defra.Lis.EventLogging.Models.Requests.Logging;
using FluentValidation.TestHelper;

public class PostEventValidatorTests
{
    private readonly PostEventValidator validator = new();

    [Fact]
    public void Should_Not_Have_Errors_When_Request_Is_Valid()
    {
        var result = this.validator.TestValidate(CreateValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("12/34/5678")]
    [InlineData("123/456/7890")]
    public void Should_Have_Error_When_Cph_Is_Invalid(string? cph)
    {
        var request = CreateValidRequest() with { CountyParishHolding = cph };

        var result = this.validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CountyParishHolding);
    }

    [Theory]
    [InlineData(nameof(PostEvent.Title))]
    [InlineData(nameof(PostEvent.Species))]
    [InlineData(nameof(PostEvent.Taxonomy))]
    [InlineData(nameof(PostEvent.SubTaxonomy))]
    [InlineData(nameof(PostEvent.CreatedBy))]
    public void Should_Have_Error_When_Required_Text_Is_Empty(string propertyName)
    {
        var request = CreateValidRequest();
        typeof(PostEvent).GetProperty(propertyName)!.SetValue(request, string.Empty);

        var result = this.validator.TestValidate(request);

        result.Errors.ShouldContain(x => x.PropertyName == propertyName);
    }

    [Theory]
    [InlineData(nameof(PostEvent.Title))]
    [InlineData(nameof(PostEvent.Species))]
    [InlineData(nameof(PostEvent.Taxonomy))]
    [InlineData(nameof(PostEvent.SubTaxonomy))]
    [InlineData(nameof(PostEvent.CreatedBy))]
    public void Should_Have_Error_When_Text_Exceeds_Maximum_Length(string propertyName)
    {
        var request = CreateValidRequest();
        typeof(PostEvent).GetProperty(propertyName)!.SetValue(request, new string('a', 51));

        var result = this.validator.TestValidate(request);

        result.Errors.ShouldContain(x => x.PropertyName == propertyName);
    }

    internal static PostEvent CreateValidRequest()
    {
        return new PostEvent()
        {
            CountyParishHolding = "12/345/6789",
            Title = "Movement submitted",
            Species = "Cattle",
            Taxonomy = "Movement",
            SubTaxonomy = "Birth",
            CreatedBy = "test-client",
        };
    }
}

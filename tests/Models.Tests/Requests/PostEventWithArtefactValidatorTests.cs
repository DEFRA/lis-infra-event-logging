// <copyright file="PostEventWithArtefactValidatorTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Tests.Requests;

using Defra.Lis.EventLogging.Models.Requests.Logging;
using FluentValidation.TestHelper;

public class PostEventWithArtefactValidatorTests
{
    private readonly PostEventWithArtefactValidator validator = new();

    [Fact]
    public void Should_Not_Have_Errors_When_Request_Is_Valid()
    {
        var result = validator.TestValidate(CreateValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Validate_The_Nested_Event()
    {
        var request = CreateValidRequest() with
        {
            Event = PostEventValidatorTests.CreateValidRequest() with { Taxonomy = string.Empty },
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Event.Taxonomy");
    }

    [Fact]
    public void Should_Validate_The_Nested_Artefact()
    {
        var request = CreateValidRequest() with
        {
            Artefact = PostArtefactValidatorTests.CreateValidRequest() with { Size = 0 },
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Artefact.Size");
    }

    private static PostEventWithArtefact CreateValidRequest()
    {
        return new PostEventWithArtefact()
        {
            Event = PostEventValidatorTests.CreateValidRequest(),
            Artefact = PostArtefactValidatorTests.CreateValidRequest(),
        };
    }
}

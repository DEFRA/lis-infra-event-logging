// <copyright file="LoggingEndpointsTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Api.Tests.Endpoints;

using System.Net;
using System.Net.Http.Headers;
using Defra.Lis.EventLogging.Api.Endpoints.Public;
using Defra.Lis.EventLogging.Api.Middleware.Headers;
using Defra.Lis.EventLogging.Models;
using Defra.Lis.EventLogging.Models.Requests.Logging;
using Defra.Lis.EventLogging.Models.Responses.Logging;
using Defra.Lis.EventLogging.Services;
using Defra.Lis.EventLogging.Services.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

public class LoggingEndpointsTests
{
    [Fact]
    public async Task SubmitEventWithArtefact_Should_Map_Multipart_File_And_Original_Filename()
    {
        var service = Substitute.For<IEventLoggingService>();
        PostEventWithArtefact? captured = null;
        service.SubmitEventWithArtefactAsync(
                Arg.Do<PostEventWithArtefact>(x => captured = x),
                Arg.Any<SubmissionContext>(),
                Arg.Any<CancellationToken>())
            .Returns(new EventSubmissionResult()
            {
                SubmissionId = Guid.NewGuid(),
                LogId = Guid.NewGuid(),
                ArtefactId = Guid.NewGuid(),
                ShortId = "EVT-ABC",
                Status = SubmissionStatus.Pending,
            });

        await using var app = await CreateApplicationAsync(service);
        using var client = app.GetTestClient();
        using var content = CreateMultipartContent();
        client.DefaultRequestHeaders.Add(RequestHeaderNames.ApiKey, "test-key");
        client.DefaultRequestHeaders.Add(RequestHeaderNames.CorrelationId, Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.Add(RequestHeaderNames.IdempotencyKey, "request-1");

        var response = await client.PostAsync(
            "/log/with-artefact",
            content,
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        captured.ShouldNotBeNull();
        captured.Event.CountyParishHolding.ShouldBe("12/345/6789");
        captured.Artefact.OriginalFilename.ShouldBe("original-report.pdf");
        captured.Artefact.MimeType.ShouldBe("application/pdf");
        captured.Artefact.Size.ShouldBe(3);
    }

    private static async Task<WebApplication> CreateApplicationAsync(IEventLoggingService service)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton(service);
        builder.Services.AddValidators();
        var app = builder.Build();
        app.UseLoggingEndpoints();
        await app.StartAsync(TestContext.Current.CancellationToken);
        return app;
    }

    private static MultipartFormDataContent CreateMultipartContent()
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent("12/345/6789"), "CountyParishHolding");
        content.Add(new StringContent("Birth event"), "Title");
        content.Add(new StringContent("test-client"), "CreatedBy");
        content.Add(new StringContent("CTT"), "Species");
        content.Add(new StringContent("BIRTH"), "Taxonomy");
        content.Add(new StringContent("DEFAULT"), "SubTaxonomy");
        var file = new ByteArrayContent([1, 2, 3]);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(file, "Artefact", "original-report.pdf");
        return content;
    }
}

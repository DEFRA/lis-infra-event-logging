// <copyright file="EventSubmissionCleanupServiceTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Worker.Tests;

using Defra.Lis.EventLogging.Repositories.Submissions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

public class EventSubmissionCleanupServiceTests
{
    [Fact]
    public async Task DeleteBatchAsync_Should_Delete_Expired_Terminal_Submissions()
    {
        var repository = Substitute.For<IEventSubmissionProcessingRepository>();
        repository.DeleteTerminalSubmissionsAsync(
                Arg.Any<DateTimeOffset>(), 25, Arg.Any<CancellationToken>())
            .Returns(3);
        var services = new ServiceCollection();
        services.AddSingleton(repository);
        var provider = services.BuildServiceProvider();
        var service = new EventSubmissionCleanupService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new QueueOptions()
            {
                SubmissionRetentionHours = 24,
                CleanupBatchSize = 25,
            }),
            Substitute.For<ILogger<EventSubmissionCleanupService>>());
        var before = DateTimeOffset.UtcNow.AddHours(-24);

        var result = await service.DeleteBatchAsync(TestContext.Current.CancellationToken);

        var after = DateTimeOffset.UtcNow.AddHours(-24);
        result.ShouldBe(3);
        await repository.Received().DeleteTerminalSubmissionsAsync(
            Arg.Is<DateTimeOffset>(value => value >= before && value <= after),
            25,
            TestContext.Current.CancellationToken);
    }
}

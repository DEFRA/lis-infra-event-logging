// <copyright file="QueueOptionsTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Worker.Tests;

public class QueueOptionsTests
{
    [Fact]
    public void Defaults_Should_Support_Long_Polling_And_Queue_Retries()
    {
        var options = new QueueOptions();

        options.WaitTimeSeconds.ShouldBe(20);
        options.VisibilityTimeoutSeconds.ShouldBe(120);
        options.OutboxBatchSize.ShouldBe(10);
        options.OutboxPollIntervalSeconds.ShouldBe(2);
    }
}

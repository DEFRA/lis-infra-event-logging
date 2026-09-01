// <copyright file="PagedEventResultTests.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Tests.Responses;

using Defra.Lis.EventLogging.Models.Responses.Logging;

public class PagedEventResultTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(25, 1)]
    [InlineData(26, 2)]
    public void TotalPages_Should_Round_Up(long totalCount, int expected)
    {
        var result = new PagedEventResult()
        {
            Items = [],
            Page = 1,
            PageSize = 25,
            TotalCount = totalCount,
        };

        result.TotalPages.ShouldBe(expected);
    }
}

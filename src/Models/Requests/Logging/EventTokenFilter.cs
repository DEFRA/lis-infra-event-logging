// <copyright file="EventTokenFilter.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Requests.Logging;

public record EventTokenFilter
{
    public required string Token { get; init; }

    public required string Value { get; init; }
}

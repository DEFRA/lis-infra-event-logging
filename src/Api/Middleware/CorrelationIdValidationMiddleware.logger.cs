// <copyright file="CorrelationIdValidationMiddleware.logger.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Api.Middleware;

public partial class CorrelationIdValidationMiddleware
{
    [LoggerMessage(LogLevel.Error, "Error in {MiddlewareName}")]
    static partial void LogErrorInMiddleware(ILogger logger, string middlewareName, Exception exception);
}

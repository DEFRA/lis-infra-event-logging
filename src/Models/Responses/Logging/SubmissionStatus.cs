// <copyright file="SubmissionStatus.cs" company="Defra">
// Copyright (c) Defra. All rights reserved.
// </copyright>

namespace Defra.Lis.EventLogging.Models.Responses.Logging;

public enum SubmissionStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
}

using System;
using System.Collections.Generic;
using SampleConsoleApp.Examples;
using Handlr.Abstractions.Queries;
using Handlr.Abstractions.Results;

namespace SampleConsoleApp.Examples;

/// <summary>
/// Example query that shows how to implement caching interfaces
/// </summary>
public record ExampleQuery : IQuery<Result<string>>, ICacheable
{
    public string UserId { get; init; } = string.Empty;

    // Required IQuery properties
    public string? CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public IDictionary<string, object>? Metadata { get; init; } = new Dictionary<string, object>();

    // Implement ICacheable to work with CachingBehaviorExample
    public string GetCacheKey() => $"user-{UserId}";

    public TimeSpan GetCacheDuration() => TimeSpan.FromMinutes(5);
}
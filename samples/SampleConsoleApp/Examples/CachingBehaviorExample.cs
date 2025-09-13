using System;
using System.Threading;
using System.Threading.Tasks;
using Handlr.Abstractions.Pipelines;

namespace SampleConsoleApp.Examples;

/// <summary>
/// Example caching behavior showing how users can implement their own caching logic.
/// This is NOT part of the framework - it's an example for users to follow.
/// This example shows a simple in-memory cache, but users could integrate with Redis, MemoryCache, etc.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class CachingBehaviorExample<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    // Simple static cache for demonstration - in real apps, use proper caching infrastructure
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (object Value, DateTime Expiry)> _cache = new();

    /// <summary>
    /// Handles the request with caching logic
    /// </summary>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
    {
        // Only cache queries that implement ICacheable
        if (request is not ICacheable cacheable)
        {
            Console.WriteLine($"[CachingBehavior] {typeof(TRequest).Name} is not cacheable, skipping cache");
            return await next();
        }

        var cacheKey = cacheable.GetCacheKey();

        // Check cache first
        if (_cache.TryGetValue(cacheKey, out var cached) && cached.Expiry > DateTime.UtcNow)
        {
            Console.WriteLine($"[CachingBehavior] Cache HIT for key: {cacheKey}");
            return (TResponse)cached.Value;
        }

        Console.WriteLine($"[CachingBehavior] Cache MISS for key: {cacheKey}");

        // Execute the handler
        var response = await next();

        // Cache the response
        var expiry = DateTime.UtcNow.Add(cacheable.GetCacheDuration());
        _cache[cacheKey] = (response!, expiry);

        Console.WriteLine($"[CachingBehavior] Cached response for key: {cacheKey}, expires: {expiry:HH:mm:ss}");

        return response;
    }
}

/// <summary>
/// Example interface that requests can implement to support caching
/// </summary>
public interface ICacheable
{
    /// <summary>
    /// Gets the cache key for this request
    /// </summary>
    string GetCacheKey();

    /// <summary>
    /// Gets how long to cache the response
    /// </summary>
    TimeSpan GetCacheDuration();
}

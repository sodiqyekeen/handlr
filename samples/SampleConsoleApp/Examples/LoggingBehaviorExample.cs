using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Handlr.Abstractions.Pipelines;

namespace SampleConsoleApp.Examples;

/// <summary>
/// Example logging behavior showing how users can implement their own logging logic.
/// This is NOT part of the framework - it's an example for users to follow.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class LoggingBehaviorExample<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Handles the request with logging before and after execution
    /// </summary>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
    {
        var requestType = typeof(TRequest).Name;
        var correlationId = Guid.NewGuid().ToString("N")[..8];
        var stopwatch = Stopwatch.StartNew();

        Console.WriteLine($"[LoggingBehavior] [{correlationId}] Starting {requestType} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

        try
        {
            // Execute the next behavior or handler
            var response = await next();

            stopwatch.Stop();
            Console.WriteLine($"[LoggingBehavior] [{correlationId}] Completed {requestType} in {stopwatch.ElapsedMilliseconds}ms");

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"[LoggingBehavior] [{correlationId}] Failed {requestType} in {stopwatch.ElapsedMilliseconds}ms - {ex.Message}");
            throw;
        }
    }
}
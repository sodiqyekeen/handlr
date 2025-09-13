using Handlr.Abstractions.Pipelines;
using Handlr.Abstractions.Results;

namespace SampleConsoleApp.Behaviors;

/// <summary>
/// Logging pipeline behavior
/// </summary>
public class LoggingBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
{
    public async Task<TResult> Handle(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        var requestName = typeof(TRequest).Name;
        Console.WriteLine($"[LOG] Handling {requestName}");

        try
        {
            var result = await next();
            Console.WriteLine($"[LOG] Completed {requestName}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LOG] Error in {requestName}: {ex.Message}");
            throw;
        }
    }
}

/// <summary>
/// Validation pipeline behavior
/// </summary>
public class ValidationBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
    where TResult : Result, new()
{
    public async Task<TResult> Handle(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        // Simple validation example
        if (request == null)
        {
            var error = new Error("ValidationError", "Request cannot be null");
            return (TResult)Result.Failure(error);
        }

        return await next();
    }
}

/// <summary>
/// Performance monitoring behavior
/// </summary>
public class PerformanceBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
{
    public async Task<TResult> Handle(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var result = await next();
            stopwatch.Stop();

            Console.WriteLine($"[PERF] {typeof(TRequest).Name} took {stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
        catch
        {
            stopwatch.Stop();
            Console.WriteLine($"[PERF] {typeof(TRequest).Name} failed after {stopwatch.ElapsedMilliseconds}ms");
            throw;
        }
    }
}
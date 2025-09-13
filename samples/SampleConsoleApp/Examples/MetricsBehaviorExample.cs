using System.Diagnostics;
using Handlr.Abstractions.Pipelines;

namespace SampleConsoleApp.Examples;

/// <summary>
/// Example interface that requests can implement to enable metrics collection
/// </summary>
public interface IMetricsEnabled
{
    /// <summary>
    /// The operation name for metrics tracking
    /// </summary>
    string OperationName { get; }

    /// <summary>
    /// Additional tags/dimensions for metrics
    /// </summary>
    Dictionary<string, string> MetricsTags { get; }
}

/// <summary>
/// Example metrics behavior that collects performance and usage metrics
/// This shows how to implement cross-cutting metrics collection
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class MetricsBehaviorExample<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMetricsEnabled
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var operationName = request.OperationName;
        var tags = request.MetricsTags;

        Console.WriteLine($"[METRICS] Starting operation: {operationName}");

        try
        {
            // Record operation start
            RecordOperationStart(operationName, tags);

            // Execute the request
            var result = await next();

            stopwatch.Stop();

            // Record successful completion
            RecordOperationSuccess(operationName, stopwatch.ElapsedMilliseconds, tags);

            Console.WriteLine($"[METRICS] Operation {operationName} completed successfully in {stopwatch.ElapsedMilliseconds}ms");

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Record failure
            RecordOperationFailure(operationName, stopwatch.ElapsedMilliseconds, ex.GetType().Name, tags);

            Console.WriteLine($"[METRICS] Operation {operationName} failed after {stopwatch.ElapsedMilliseconds}ms with {ex.GetType().Name}");

            throw;
        }
    }

    private void RecordOperationStart(string operationName, Dictionary<string, string> tags)
    {
        // In a real implementation, you would send metrics to your monitoring system:
        // - Prometheus
        // - Application Insights
        // - CloudWatch
        // - StatsD
        // - Custom metrics endpoint

        Console.WriteLine($"[METRICS] 📊 Counter: {operationName}.started +1");

        foreach (var tag in tags)
        {
            Console.WriteLine($"[METRICS] 🏷️  Tag: {tag.Key}={tag.Value}");
        }
    }

    private void RecordOperationSuccess(string operationName, long durationMs, Dictionary<string, string> tags)
    {
        // Example metrics you might collect:
        Console.WriteLine($"[METRICS] 📊 Counter: {operationName}.completed +1");
        Console.WriteLine($"[METRICS] ⏱️  Histogram: {operationName}.duration {durationMs}ms");
        Console.WriteLine($"[METRICS] ✅ Counter: {operationName}.success +1");

        // Performance categorization
        var performanceCategory = durationMs switch
        {
            < 100 => "fast",
            < 500 => "medium",
            < 2000 => "slow",
            _ => "very_slow"
        };

        Console.WriteLine($"[METRICS] 🚀 Counter: {operationName}.performance.{performanceCategory} +1");

        // Track percentiles (in real implementation)
        RecordPercentiles(operationName, durationMs);
    }

    private void RecordOperationFailure(string operationName, long durationMs, string exceptionType, Dictionary<string, string> tags)
    {
        Console.WriteLine($"[METRICS] 📊 Counter: {operationName}.completed +1");
        Console.WriteLine($"[METRICS] ⏱️  Histogram: {operationName}.duration {durationMs}ms");
        Console.WriteLine($"[METRICS] ❌ Counter: {operationName}.failed +1");
        Console.WriteLine($"[METRICS] 🚨 Counter: {operationName}.error.{exceptionType} +1");

        // Error rate calculation (example)
        // In practice, you'd track this over time windows
        Console.WriteLine($"[METRICS] 📈 Gauge: {operationName}.error_rate (would be calculated)");
    }

    private void RecordPercentiles(string operationName, long durationMs)
    {
        // In a real implementation, you'd maintain histograms for percentile calculation
        // This is just for demonstration
        Console.WriteLine($"[METRICS] 📏 Percentile data recorded for {operationName} ({durationMs}ms)");
        Console.WriteLine($"[METRICS] 📊 Future percentiles: p50, p90, p95, p99 for {operationName}");
    }
}

/// <summary>
/// Example command with metrics enabled
/// </summary>
public class MetricsEnabledCommand : IMetricsEnabled
{
    public string Action { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;

    public string OperationName => "user.command.execute";

    public Dictionary<string, string> MetricsTags => new()
    {
        { "command_type", nameof(MetricsEnabledCommand) },
        { "action", Action },
        { "user_id", UserId },
        { "environment", "development" }
    };
}

/// <summary>
/// Example query with metrics enabled  
/// </summary>
public class MetricsEnabledQuery : IMetricsEnabled
{
    public string Resource { get; set; } = string.Empty;
    public string Filter { get; set; } = string.Empty;

    public string OperationName => "data.query.execute";

    public Dictionary<string, string> MetricsTags => new()
    {
        { "query_type", nameof(MetricsEnabledQuery) },
        { "resource", Resource },
        { "has_filter", (!string.IsNullOrEmpty(Filter)).ToString().ToLower() },
        { "environment", "development" }
    };
}

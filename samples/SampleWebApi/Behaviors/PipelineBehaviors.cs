using Handlr.Abstractions.Pipelines;
using System.Diagnostics;

namespace SampleWebApi.Behaviors;

/// <summary>
/// Logging behavior that logs request execution details
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var correlationId = Guid.NewGuid().ToString()[..8];

        _logger.LogInformation(
            "[{CorrelationId}] Starting {RequestName} at {Timestamp}",
            correlationId, requestName, DateTime.UtcNow);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();
            _logger.LogInformation(
                "[{CorrelationId}] Completed {RequestName} in {ElapsedMs}ms",
                correlationId, requestName, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "[{CorrelationId}] Failed {RequestName} after {ElapsedMs}ms: {ErrorMessage}",
                correlationId, requestName, stopwatch.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }
}

/// <summary>
/// Validation behavior that performs basic request validation
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Basic null check
        if (request == null)
        {
            _logger.LogWarning("Validation failed for {RequestName}: Request is null", requestName);
            throw new ArgumentNullException(nameof(request));
        }

        _logger.LogDebug("Validation passed for {RequestName}", requestName);

        return await next();
    }
}

/// <summary>
/// Metrics behavior that collects performance metrics
/// </summary>
public class MetricsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<MetricsBehavior<TRequest, TResponse>> _logger;

    public MetricsBehavior(ILogger<MetricsBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            // Log metrics (in a real app, you'd send these to a metrics system)
            _logger.LogInformation(
                "METRIC: {RequestName} executed in {ElapsedMs}ms [Success]",
                requestName, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception)
        {
            stopwatch.Stop();

            _logger.LogWarning(
                "METRIC: {RequestName} failed after {ElapsedMs}ms [Error]",
                requestName, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
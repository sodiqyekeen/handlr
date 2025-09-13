using Handlr.Abstractions.Pipelines;

namespace SampleConsoleApp.Examples;

/// <summary>
/// Example interface that requests can implement to enable retry behavior
/// </summary>
public interface IRetryable
{
    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    int MaxRetries { get; }

    /// <summary>
    /// Delay between retries in milliseconds
    /// </summary>
    int RetryDelayMs { get; }

    /// <summary>
    /// Whether to use exponential backoff
    /// </summary>
    bool UseExponentialBackoff { get; }
}

/// <summary>
/// Example retry behavior that handles transient failures
/// This shows how to implement retry logic with exponential backoff
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class RetryBehaviorExample<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRetryable
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var attempt = 0;
        var maxRetries = request.MaxRetries;
        var delayMs = request.RetryDelayMs;

        Console.WriteLine($"[RETRY] Starting execution of {typeof(TRequest).Name} with up to {maxRetries} retries");

        while (attempt <= maxRetries)
        {
            try
            {
                if (attempt > 0)
                {
                    Console.WriteLine($"[RETRY] Attempt {attempt} of {maxRetries} for {typeof(TRequest).Name}");
                }

                // Execute the request
                var result = await next();

                if (attempt > 0)
                {
                    Console.WriteLine($"[RETRY] Success on attempt {attempt} for {typeof(TRequest).Name}");
                }

                return result;
            }
            catch (Exception ex) when (ShouldRetry(ex, attempt, maxRetries))
            {
                attempt++;

                if (attempt <= maxRetries)
                {
                    var currentDelay = request.UseExponentialBackoff
                        ? CalculateExponentialBackoff(delayMs, attempt)
                        : delayMs;

                    Console.WriteLine($"[RETRY] Attempt {attempt - 1} failed for {typeof(TRequest).Name}: {ex.Message}");
                    Console.WriteLine($"[RETRY] Retrying in {currentDelay}ms (attempt {attempt}/{maxRetries})");

                    await Task.Delay(currentDelay, cancellationToken);
                }
                else
                {
                    Console.WriteLine($"[RETRY] All {maxRetries} retry attempts failed for {typeof(TRequest).Name}");
                    throw;
                }
            }
        }

        // This should never be reached, but compiler requires it
        throw new InvalidOperationException("Retry logic error");
    }

    private static bool ShouldRetry(Exception exception, int currentAttempt, int maxRetries)
    {
        if (currentAttempt >= maxRetries)
            return false;

        // Example: Only retry on specific exception types
        // In practice, you might want to retry on:
        // - HttpRequestException
        // - SocketException  
        // - TimeoutException
        // - SqlException (for transient SQL errors)
        // - Custom transient exceptions

        return exception is TimeoutException
            || exception is InvalidOperationException
            || exception.Message.Contains("transient", StringComparison.OrdinalIgnoreCase);
    }

    private static int CalculateExponentialBackoff(int baseDelayMs, int attempt)
    {
        // Exponential backoff: delay * (2^attempt) with some jitter
        var exponentialDelay = baseDelayMs * Math.Pow(2, attempt - 1);

        // Add some jitter to prevent thundering herd
        var jitter = new Random().NextDouble() * 0.1 + 0.9; // 90-100% of calculated delay

        return (int)(exponentialDelay * jitter);
    }
}

/// <summary>
/// Example command that can be retried on failure
/// </summary>
public class RetryableCommand : IRetryable
{
    public string Operation { get; set; } = string.Empty;
    public bool SimulateFailure { get; set; } = false;

    public int MaxRetries => 3;
    public int RetryDelayMs => 500;
    public bool UseExponentialBackoff => true;
}

/// <summary>
/// Example query that can be retried (useful for external API calls)
/// </summary>
public class RetryableQuery : IRetryable
{
    public string ExternalResource { get; set; } = string.Empty;

    public int MaxRetries => 2;
    public int RetryDelayMs => 1000;
    public bool UseExponentialBackoff => false; // Simple fixed delay for queries
}
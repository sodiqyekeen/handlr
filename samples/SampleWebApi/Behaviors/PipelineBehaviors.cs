using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using FluentValidation;
using Handlr.Abstractions.Pipelines;

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
/// Universal validation behavior that automatically detects and handles ALL validation frameworks
/// - FluentValidation: Uses registered IValidator&lt;T&gt; instances
/// - Data Annotations: Uses System.ComponentModel.DataAnnotations attributes
/// - Custom Validators: Can be extended for any validation pattern
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ValidationBehavior(ILogger<ValidationBehavior<TRequest, TResponse>> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
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

        var allValidationErrors = new List<ValidationError>();

        // 1. Try FluentValidation (requires registration)
        await TryFluentValidation(request, allValidationErrors, requestName, cancellationToken);

        // 2. Try Data Annotations (no registration needed)
        TryDataAnnotationsValidation(request, allValidationErrors, requestName);

        // 3. Try Custom Validators (extensible pattern)
        await TryCustomValidation(request, allValidationErrors, requestName, cancellationToken);

        // If any validation failed, throw exception
        if (allValidationErrors.Count > 0)
        {
            var errorMessage = string.Join("; ", allValidationErrors.Select(e => e.ErrorMessage));
            _logger.LogWarning("❌ Validation failed for {RequestName}: {Errors}", requestName, errorMessage);

            // Create FluentValidation exception for backward compatibility
            var fluentValidationErrors = allValidationErrors.Select(e =>
                new FluentValidation.Results.ValidationFailure(e.PropertyName, e.ErrorMessage));

            throw new FluentValidation.ValidationException(fluentValidationErrors);
        }

        _logger.LogDebug("✅ Validation passed for {RequestName}", requestName);
        return await next();
    }

    /// <summary>
    /// Try FluentValidation if validators are registered
    /// </summary>
    private async Task TryFluentValidation(TRequest request, List<ValidationError> errors, string requestName, CancellationToken cancellationToken)
    {
        try
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(typeof(TRequest));
            var validator = _serviceProvider.GetService(validatorType) as IValidator<TRequest>;

            if (validator != null)
            {
                _logger.LogDebug("🔍 Found FluentValidation validator for {RequestName}", requestName);

                var result = await validator.ValidateAsync(request, cancellationToken);
                if (!result.IsValid)
                {
                    var fluentErrors = result.Errors.Select(e => new ValidationError
                    {
                        PropertyName = e.PropertyName,
                        ErrorMessage = e.ErrorMessage
                    });
                    errors.AddRange(fluentErrors);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error during FluentValidation for {RequestName}: {Error}", requestName, ex.Message);
        }
    }

    /// <summary>
    /// Try Data Annotations validation (always available, no registration needed)
    /// </summary>
    private void TryDataAnnotationsValidation(TRequest request, List<ValidationError> errors, string requestName)
    {
        try
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(request!);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            var isValid = Validator.TryValidateObject(request!, validationContext, validationResults, true);

            if (!isValid)
            {
                _logger.LogDebug("🔍 Found Data Annotations validation errors for {RequestName}", requestName);

                var dataAnnotationErrors = validationResults.Select(r => new ValidationError
                {
                    PropertyName = r.MemberNames.FirstOrDefault() ?? string.Empty,
                    ErrorMessage = r.ErrorMessage ?? "Validation failed"
                });
                errors.AddRange(dataAnnotationErrors);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error during Data Annotations validation for {RequestName}: {Error}", requestName, ex.Message);
        }
    }

    /// <summary>
    /// Try custom validation patterns (extensible for future frameworks)
    /// </summary>
    private async Task TryCustomValidation(TRequest request, List<ValidationError> errors, string requestName, CancellationToken cancellationToken)
    {
        try
        {
            // Look for any custom validators that implement IValidationStrategy<TRequest>
            var customValidators = _serviceProvider.GetServices<IValidationStrategy<TRequest>>();

            foreach (var validator in customValidators)
            {
                _logger.LogDebug("🔍 Found custom validator for {RequestName}: {ValidatorType}", requestName, validator.GetType().Name);

                var result = await validator.ValidateAsync(request, cancellationToken);
                if (!result.IsValid)
                {
                    errors.AddRange(result.Errors);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error during custom validation for {RequestName}: {Error}", requestName, ex.Message);
        }
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

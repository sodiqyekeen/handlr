using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Handlr.Abstractions.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Handlr.Extensions.FluentValidation;

/// <summary>
/// FluentValidation pipeline behavior for Handlr CQRS framework.
/// This behavior automatically validates commands and queries using registered FluentValidation validators.
/// </summary>
/// <typeparam name="TRequest">The request type (command or query)</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class FluentValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IValidator<TRequest>? _validator;
    private readonly ILogger<FluentValidationBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of the FluentValidationBehavior class.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve validators</param>
    /// <param name="logger">The logger instance</param>
    public FluentValidationBehavior(IServiceProvider serviceProvider, ILogger<FluentValidationBehavior<TRequest, TResponse>> logger)
    {
        _validator = serviceProvider.GetService<IValidator<TRequest>>();
        _logger = logger;
    }

    /// <summary>
    /// Handles the request by performing FluentValidation before proceeding to the next behavior or handler.
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <param name="next">The next behavior or handler in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The response from the next behavior or handler</returns>
    /// <exception cref="ValidationException">Thrown when validation fails</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // If no validator is registered, skip validation
        if (_validator == null)
        {
            _logger.LogDebug("No FluentValidation validator found for {RequestType}, skipping validation", requestName);
            return await next();
        }

        _logger.LogDebug("Executing FluentValidation for {RequestType}", requestName);

        // Perform validation
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToList();
            _logger.LogWarning("FluentValidation failed for {RequestType}: {Errors}", requestName, string.Join(", ", errors));

            throw new ValidationException(validationResult.Errors);
        }

        _logger.LogDebug("FluentValidation passed for {RequestType}", requestName);
        return await next();
    }
}

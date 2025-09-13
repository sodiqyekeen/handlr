using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Handlr.Abstractions.Pipelines;

namespace SampleConsoleApp.Examples;

/// <summary>
/// Example validation behavior showing how users can implement their own validation logic.
/// This is NOT part of the framework - it's an example for users to follow.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class ValidationBehaviorExample<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
    {
        // Example validation logic - users would implement their own
        Console.WriteLine($"[ValidationBehavior] Validating {typeof(TRequest).Name}");

        // Example: Check if request implements a validation interface
        if (request is IValidatable validatable)
        {
            var validationResult = validatable.Validate();
            if (!validationResult.IsValid)
            {
                Console.WriteLine($"[ValidationBehavior] Validation failed: {string.Join(", ", validationResult.Errors)}");
                // In a real implementation, you might throw an exception or return an error response
                // throw new ValidationException(validationResult.Errors);
            }
        }

        Console.WriteLine($"[ValidationBehavior] Validation passed for {typeof(TRequest).Name}");

        // Continue to the next behavior or handler
        return await next();
    }
}

/// <summary>
/// Example interface that requests can implement to support validation
/// </summary>
public interface IValidatable
{
    ValidationResult Validate();
}

/// <summary>
/// Example validation result class
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; }
    public IReadOnlyList<string> Errors { get; }

    public ValidationResult(bool isValid, IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Errors = errors ?? new List<string>();
    }

    public static ValidationResult Success() => new(true, new List<string>());
    public static ValidationResult Failure(params string[] errors) => new(false, errors);
}

#nullable enable
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace SampleWebApi.Behaviors;

/// <summary>
/// Base interface for validation strategies
/// </summary>
public interface IValidationStrategy<TRequest>
{
    /// <summary>
    /// Validates the request and returns validation results
    /// </summary>
    Task<ValidationResult> ValidateAsync(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of a validation operation
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();

    public static ValidationResult Success() => new() { IsValid = true };

    public static ValidationResult Failure(IEnumerable<ValidationError> errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };
}

/// <summary>
/// Represents a validation error
/// </summary>
public class ValidationError
{
    public string PropertyName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// FluentValidation strategy implementation
/// </summary>
public class FluentValidationStrategy<TRequest> : IValidationStrategy<TRequest>
{
    private readonly IValidator<TRequest>? _validator;

    public FluentValidationStrategy(IServiceProvider serviceProvider)
    {
        _validator = serviceProvider.GetService<IValidator<TRequest>>();
    }

    public async Task<ValidationResult> ValidateAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        if (_validator == null)
            return ValidationResult.Success();

        var result = await _validator.ValidateAsync(request, cancellationToken);

        if (result.IsValid)
            return ValidationResult.Success();

        var errors = result.Errors.Select(e => new ValidationError
        {
            PropertyName = e.PropertyName,
            ErrorMessage = e.ErrorMessage
        });

        return ValidationResult.Failure(errors);
    }
}

/// <summary>
/// Data Annotations validation strategy implementation
/// </summary>
public class DataAnnotationsValidationStrategy<TRequest> : IValidationStrategy<TRequest>
{
    public Task<ValidationResult> ValidateAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(request!);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        var isValid = Validator.TryValidateObject(request!, validationContext, validationResults, true);

        if (isValid)
            return Task.FromResult(ValidationResult.Success());

        var errors = validationResults.Select(r => new ValidationError
        {
            PropertyName = r.MemberNames.FirstOrDefault() ?? string.Empty,
            ErrorMessage = r.ErrorMessage ?? "Validation failed"
        });

        return Task.FromResult(ValidationResult.Failure(errors));
    }
}

/// <summary>
/// Composite validation strategy that tries multiple validation approaches
/// </summary>
public class CompositeValidationStrategy<TRequest> : IValidationStrategy<TRequest>
{
    private readonly List<IValidationStrategy<TRequest>> _strategies;

    public CompositeValidationStrategy(IServiceProvider serviceProvider)
    {
        _strategies = new List<IValidationStrategy<TRequest>>
        {
            new FluentValidationStrategy<TRequest>(serviceProvider),
            new DataAnnotationsValidationStrategy<TRequest>()
        };
    }

    public async Task<ValidationResult> ValidateAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        var allErrors = new List<ValidationError>();

        foreach (var strategy in _strategies)
        {
            var result = await strategy.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
            {
                allErrors.AddRange(result.Errors);
            }
        }

        return allErrors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(allErrors);
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Handlr.Abstractions.Pipelines;
using Microsoft.Extensions.Logging;

namespace Handlr.Extensions.DataAnnotations;

/// <summary>
/// Data Annotations pipeline behavior for Handlr CQRS framework.
/// This behavior automatically validates commands and queries using System.ComponentModel.DataAnnotations attributes.
/// </summary>
/// <typeparam name="TRequest">The request type (command or query)</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class DataAnnotationsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<DataAnnotationsBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of the DataAnnotationsBehavior class.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public DataAnnotationsBehavior(ILogger<DataAnnotationsBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the request by performing Data Annotations validation before proceeding to the next behavior or handler.
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <param name="next">The next behavior or handler in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The response from the next behavior or handler</returns>
    /// <exception cref="DataAnnotationsValidationException">Thrown when validation fails</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        if (request == null)
        {
            _logger.LogWarning("Validation failed for {RequestType}: Request is null", requestName);
            throw new ArgumentNullException(nameof(request));
        }

        _logger.LogDebug("Executing Data Annotations validation for {RequestType}", requestName);

        // Perform Data Annotations validation (including nested objects)
        var validationResults = new List<ValidationResult>();
        var isValid = TryValidateObjectRecursive(request, validationResults);

        if (!isValid)
        {
            var errors = validationResults.Select(r =>
                $"{string.Join(", ", r.MemberNames)}: {r.ErrorMessage}").ToList();

            _logger.LogWarning("Data Annotations validation failed for {RequestType}: {Errors}",
                requestName, string.Join("; ", errors));

            throw new DataAnnotationsValidationException(validationResults);
        }

        _logger.LogDebug("Data Annotations validation passed for {RequestType}", requestName);
        return await next();
    }

    /// <summary>
    /// Recursively validates an object and its nested properties using Data Annotations.
    /// </summary>
    /// <param name="obj">The object to validate</param>
    /// <param name="results">The list to collect validation results</param>
    /// <param name="validatedObjects">Set of already validated objects to prevent infinite recursion</param>
    /// <returns>True if validation passed, false otherwise</returns>
    private bool TryValidateObjectRecursive(object obj, IList<ValidationResult> results, HashSet<object>? validatedObjects = null)
    {
        if (obj == null) return true;

        validatedObjects ??= new HashSet<object>();

        // Prevent infinite recursion
        if (validatedObjects.Contains(obj)) return true;
        validatedObjects.Add(obj);

        var context = new ValidationContext(obj);
        var isValid = Validator.TryValidateObject(obj, context, results, validateAllProperties: true);

        // Validate nested complex objects
        var properties = obj.GetType().GetProperties()
            .Where(prop => prop.CanRead && prop.GetGetMethod() != null);

        foreach (var property in properties)
        {
            var value = property.GetValue(obj);
            if (value == null) continue;

            var valueType = value.GetType();

            // Skip primitive types, strings, and value types
            if (valueType.IsPrimitive || valueType == typeof(string) || valueType.IsValueType) continue;

            // Handle collections
            if (value is System.Collections.IEnumerable enumerable && !(value is string))
            {
                foreach (var item in enumerable)
                {
                    if (item != null && !TryValidateObjectRecursive(item, results, validatedObjects))
                    {
                        isValid = false;
                    }
                }
            }
            // Handle single complex objects
            else if (valueType.IsClass)
            {
                if (!TryValidateObjectRecursive(value, results, validatedObjects))
                {
                    isValid = false;
                }
            }
        }

        return isValid;
    }
}

/// <summary>
/// Exception thrown when Data Annotations validation fails.
/// </summary>
public class DataAnnotationsValidationException : Exception
{
    /// <summary>
    /// Gets the validation results that caused the exception.
    /// </summary>
    public IReadOnlyList<ValidationResult> ValidationResults { get; }

    /// <summary>
    /// Initializes a new instance of the DataAnnotationsValidationException class.
    /// </summary>
    /// <param name="validationResults">The validation results that failed</param>
    public DataAnnotationsValidationException(IEnumerable<ValidationResult> validationResults)
        : base($"Data Annotations validation failed: {string.Join("; ", validationResults.Select(r => r.ErrorMessage))}")
    {
        ValidationResults = validationResults.ToList().AsReadOnly();
    }

    /// <summary>
    /// Initializes a new instance of the DataAnnotationsValidationException class.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="validationResults">The validation results that failed</param>
    public DataAnnotationsValidationException(string message, IEnumerable<ValidationResult> validationResults)
        : base(message)
    {
        ValidationResults = validationResults.ToList().AsReadOnly();
    }
}

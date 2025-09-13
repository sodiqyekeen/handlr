using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Handlr.Abstractions.Validation;

/// <summary>
/// Interface for validating objects. Optional utility for users who want validation support.
/// </summary>
/// <typeparam name="T">The type of object to validate</typeparam>
public interface IValidator<in T>
{
    /// <summary>
    /// Validates the specified object.
    /// </summary>
    /// <param name="instance">The object to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous validation operation with the validation result</returns>
    Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the validation was successful.
    /// </summary>
    public bool IsValid { get; private set; }

    /// <summary>
    /// Gets the collection of validation errors.
    /// </summary>
    public IReadOnlyList<ValidationError> Errors { get; private set; }

    /// <summary>
    /// Initializes a new instance of the ValidationResult class.
    /// </summary>
    /// <param name="errors">The validation errors</param>
    public ValidationResult(IEnumerable<ValidationError>? errors = null)
    {
        var errorList = errors?.ToList() ?? new List<ValidationError>();
        Errors = errorList;
        IsValid = errorList.Count == 0;
    }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A successful ValidationResult</returns>
    public static ValidationResult Success()
    {
        return new ValidationResult();
    }

    /// <summary>
    /// Creates a failed validation result with errors.
    /// </summary>
    /// <param name="errors">The validation errors</param>
    /// <returns>A failed ValidationResult</returns>
    public static ValidationResult Failure(IEnumerable<ValidationError> errors)
    {
        return new ValidationResult(errors);
    }

    /// <summary>
    /// Creates a failed validation result with a single error.
    /// </summary>
    /// <param name="error">The validation error</param>
    /// <returns>A failed ValidationResult</returns>
    public static ValidationResult Failure(ValidationError error)
    {
        return new ValidationResult(new[] { error });
    }

    /// <summary>
    /// Creates a failed validation result with a simple error message.
    /// </summary>
    /// <param name="propertyName">The property name</param>
    /// <param name="errorMessage">The error message</param>
    /// <returns>A failed ValidationResult</returns>
    public static ValidationResult Failure(string propertyName, string errorMessage)
    {
        return Failure(new ValidationError(propertyName, errorMessage));
    }
}

/// <summary>
/// Represents a validation error for a specific property.
/// </summary>
/// <remarks>
/// Initializes a new instance of the ValidationError class.
/// </remarks>
/// <param name="propertyName">The property name</param>
/// <param name="errorMessage">The error message</param>
/// <param name="errorCode">The error code (optional)</param>
/// <param name="attemptedValue">The attempted value (optional)</param>
public class ValidationError(string propertyName, string errorMessage, string? errorCode = null, object? attemptedValue = null) : IEquatable<ValidationError>
{
    /// <summary>
    /// Gets the name of the property that failed validation.
    /// </summary>
    public string PropertyName { get; } = propertyName ?? throw new ArgumentNullException(nameof(propertyName));

    /// <summary>
    /// Gets the validation error message.
    /// </summary>
    public string ErrorMessage { get; } = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));

    /// <summary>
    /// Gets the error code (optional).
    /// </summary>
    public string? ErrorCode { get; } = errorCode;

    /// <summary>
    /// Gets the attempted value that failed validation (optional).
    /// </summary>
    public object? AttemptedValue { get; } = attemptedValue;

    /// <summary>
    /// Determines whether the specified ValidationError is equal to the current ValidationError.
    /// </summary>
    /// <param name="other">The ValidationError to compare with the current ValidationError</param>
    /// <returns>true if the specified ValidationError is equal to the current ValidationError; otherwise, false</returns>
    public bool Equals(ValidationError? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return PropertyName == other.PropertyName && ErrorMessage == other.ErrorMessage && ErrorCode == other.ErrorCode;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current ValidationError.
    /// </summary>
    /// <param name="obj">The object to compare with the current ValidationError</param>
    /// <returns>true if the specified object is equal to the current ValidationError; otherwise, false</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as ValidationError);
    }

    /// <summary>
    /// Returns the hash code for this ValidationError.
    /// </summary>
    /// <returns>A hash code for the current ValidationError</returns>
    public override int GetHashCode()
    {
        return (PropertyName?.GetHashCode() ?? 0) ^ (ErrorMessage?.GetHashCode() ?? 0) ^ (ErrorCode?.GetHashCode() ?? 0);
    }

    /// <summary>
    /// Returns a string representation of the ValidationError.
    /// </summary>
    /// <returns>A string representation of the ValidationError</returns>
    public override string ToString()
    {
        return $"{PropertyName}: {ErrorMessage}";
    }

    /// <summary>
    /// Determines whether two ValidationError instances are equal.
    /// </summary>
    /// <param name="left">The first ValidationError to compare</param>
    /// <param name="right">The second ValidationError to compare</param>
    /// <returns>true if the ValidationError instances are equal; otherwise, false</returns>
    public static bool operator ==(ValidationError? left, ValidationError? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Determines whether two ValidationError instances are not equal.
    /// </summary>
    /// <param name="left">The first ValidationError to compare</param>
    /// <param name="right">The second ValidationError to compare</param>
    /// <returns>true if the ValidationError instances are not equal; otherwise, false</returns>
    public static bool operator !=(ValidationError? left, ValidationError? right)
    {
        return !Equals(left, right);
    }
}

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
/// <remarks>
/// Initializes a new instance of the ValidationException class.
/// </remarks>
/// <param name="errors">The validation errors</param>
public class ValidationException(IEnumerable<ValidationError> errors) : Exception("One or more validation errors occurred.")
{
    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public IReadOnlyList<ValidationError> Errors { get; } = errors?.ToList() ?? throw new ArgumentNullException(nameof(errors));

    /// <summary>
    /// Initializes a new instance of the ValidationException class with a single error.
    /// </summary>
    /// <param name="error">The validation error</param>
    public ValidationException(ValidationError error)
        : this([error])
    {
    }

    /// <summary>
    /// Initializes a new instance of the ValidationException class with a simple error.
    /// </summary>
    /// <param name="propertyName">The property name</param>
    /// <param name="errorMessage">The error message</param>
    public ValidationException(string propertyName, string errorMessage)
        : this(new ValidationError(propertyName, errorMessage))
    {
    }

    /// <summary>
    /// Initializes a new instance of the ValidationException class with a validation result.
    /// </summary>
    /// <param name="validationResult">The validation result</param>
    public ValidationException(ValidationResult validationResult)
        : this(validationResult?.Errors ?? throw new ArgumentNullException(nameof(validationResult)))
    {
    }
}

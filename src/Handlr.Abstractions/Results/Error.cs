using System;
using System.Collections.Generic;

namespace Handlr.Abstractions.Results;

/// <summary>
/// Represents an error that can occur during request processing.
/// Optional utility class for users who want structured error handling.
/// </summary>
/// <remarks>
/// Initializes a new instance of the Error class.
/// </remarks>
/// <param name="code">The error code</param>
/// <param name="message">The error message</param>
/// <param name="type">The error type</param>
/// <param name="metadata">Additional metadata</param>
public class Error(string code, string message, ErrorType type = ErrorType.General, IReadOnlyDictionary<string, object>? metadata = null) : IEquatable<Error>
{
    /// <summary>
    /// Gets the error code.
    /// </summary>
    public string Code { get; } = code ?? throw new ArgumentNullException(nameof(code));

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string Message { get; } = message ?? throw new ArgumentNullException(nameof(message));

    /// <summary>
    /// Gets the error type.
    /// </summary>
    public ErrorType Type { get; } = type;

    /// <summary>
    /// Gets additional metadata associated with the error.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Metadata { get; } = metadata;

    /// <summary>
    /// Creates a general error.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="code">The error code (optional)</param>
    /// <returns>A general error</returns>
    public static Error General(string message, string? code = null)
    {
        return new Error(code ?? "GENERAL_ERROR", message, ErrorType.General);
    }

    /// <summary>
    /// Creates a validation error.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="code">The error code (optional)</param>
    /// <param name="propertyName">The property name that failed validation</param>
    /// <returns>A validation error</returns>
    public static Error Validation(string message, string? code = null, string? propertyName = null)
    {
        var metadata = propertyName != null
            ? new Dictionary<string, object> { ["PropertyName"] = propertyName }
            : null;

        return new Error(code ?? "VALIDATION_ERROR", message, ErrorType.Validation, metadata);
    }

    /// <summary>
    /// Creates a not found error.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="code">The error code (optional)</param>
    /// <returns>A not found error</returns>
    public static Error NotFound(string message, string? code = null)
    {
        return new Error(code ?? "NOT_FOUND", message, ErrorType.NotFound);
    }

    /// <summary>
    /// Creates an unauthorized error.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="code">The error code (optional)</param>
    /// <returns>An unauthorized error</returns>
    public static Error Unauthorized(string message, string? code = null)
    {
        return new Error(code ?? "UNAUTHORIZED", message, ErrorType.Unauthorized);
    }

    /// <summary>
    /// Creates a forbidden error.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="code">The error code (optional)</param>
    /// <returns>A forbidden error</returns>
    public static Error Forbidden(string message, string? code = null)
    {
        return new Error(code ?? "FORBIDDEN", message, ErrorType.Forbidden);
    }

    /// <summary>
    /// Creates a conflict error.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="code">The error code (optional)</param>
    /// <returns>A conflict error</returns>
    public static Error Conflict(string message, string? code = null)
    {
        return new Error(code ?? "CONFLICT", message, ErrorType.Conflict);
    }

    /// <summary>
    /// Creates an internal error.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="code">The error code (optional)</param>
    /// <returns>An internal error</returns>
    public static Error Internal(string message, string? code = null)
    {
        return new Error(code ?? "INTERNAL_ERROR", message, ErrorType.Internal);
    }

    /// <summary>
    /// Determines whether the specified Error is equal to the current Error.
    /// </summary>
    /// <param name="other">The Error to compare with the current Error</param>
    /// <returns>true if the specified Error is equal to the current Error; otherwise, false</returns>
    public bool Equals(Error? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Code == other.Code && Message == other.Message && Type == other.Type;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current Error.
    /// </summary>
    /// <param name="obj">The object to compare with the current Error</param>
    /// <returns>true if the specified object is equal to the current Error; otherwise, false</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as Error);
    }

    /// <summary>
    /// Returns the hash code for this Error.
    /// </summary>
    /// <returns>A hash code for the current Error</returns>
    public override int GetHashCode()
    {
        return (Code?.GetHashCode() ?? 0) ^ (Message?.GetHashCode() ?? 0) ^ Type.GetHashCode();
    }

    /// <summary>
    /// Returns a string representation of the Error.
    /// </summary>
    /// <returns>A string representation of the Error</returns>
    public override string ToString()
    {
        return $"[{Type}] {Code}: {Message}";
    }

    /// <summary>
    /// Determines whether two Error instances are equal.
    /// </summary>
    /// <param name="left">The first Error to compare</param>
    /// <param name="right">The second Error to compare</param>
    /// <returns>true if the Error instances are equal; otherwise, false</returns>
    public static bool operator ==(Error? left, Error? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Determines whether two Error instances are not equal.
    /// </summary>
    /// <param name="left">The first Error to compare</param>
    /// <param name="right">The second Error to compare</param>
    /// <returns>true if the Error instances are not equal; otherwise, false</returns>
    public static bool operator !=(Error? left, Error? right)
    {
        return !Equals(left, right);
    }
}

/// <summary>
/// Enumeration of error types.
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// General error type.
    /// </summary>
    General,

    /// <summary>
    /// Validation error type.
    /// </summary>
    Validation,

    /// <summary>
    /// Not found error type.
    /// </summary>
    NotFound,

    /// <summary>
    /// Unauthorized error type.
    /// </summary>
    Unauthorized,

    /// <summary>
    /// Forbidden error type.
    /// </summary>
    Forbidden,

    /// <summary>
    /// Conflict error type.
    /// </summary>
    Conflict,

    /// <summary>
    /// Internal error type.
    /// </summary>
    Internal
}

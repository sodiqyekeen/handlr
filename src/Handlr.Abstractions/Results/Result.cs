using System;
using System.Collections.Generic;
using System.Linq;

namespace Handlr.Abstractions.Results;

/// <summary>
/// Optional Result pattern implementation. Users are NOT required to use this - they can return any type they want.
/// This is provided as a convenience for users who prefer the Result pattern.
/// </summary>
/// <typeparam name="T">The type of the success value</typeparam>
public class Result<T>
{
    /// <summary>
    /// Gets a value indicating whether the result represents a success.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the result represents a failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the success value. Only available when IsSuccess is true.
    /// </summary>
    public T? Value { get; private set; }

    /// <summary>
    /// Gets the collection of errors. Only populated when IsFailure is true.
    /// </summary>
    public IReadOnlyList<Error> Errors { get; private set; } = new List<Error>();

    /// <summary>
    /// Gets the first error or null if no errors exist.
    /// </summary>
    public Error? FirstError => Errors.FirstOrDefault();

    /// <summary>
    /// Private constructor for creating Result instances.
    /// </summary>
    private Result(bool isSuccess, T? value, IEnumerable<Error>? errors = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors?.ToList() ?? new List<Error>();
    }

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <param name="value">The success value</param>
    /// <returns>A successful Result</returns>
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value);
    }

    /// <summary>
    /// Creates a failed result with a single error.
    /// </summary>
    /// <param name="error">The error</param>
    /// <returns>A failed Result</returns>
    public static Result<T> Failure(Error error)
    {
        return new Result<T>(false, default, new[] { error });
    }

    /// <summary>
    /// Creates a failed result with multiple errors.
    /// </summary>
    /// <param name="errors">The collection of errors</param>
    /// <returns>A failed Result</returns>
    public static Result<T> Failure(IEnumerable<Error> errors)
    {
        return new Result<T>(false, default, errors);
    }

    /// <summary>
    /// Creates a failed result with a simple error message.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <returns>A failed Result</returns>
    public static Result<T> Failure(string message)
    {
        return Failure(Error.General(message));
    }

    /// <summary>
    /// Implicit conversion from value to successful Result.
    /// </summary>
    /// <param name="value">The value</param>
    public static implicit operator Result<T>(T value)
    {
        return Success(value);
    }

    /// <summary>
    /// Implicit conversion from Error to failed Result.
    /// </summary>
    /// <param name="error">The error</param>
    public static implicit operator Result<T>(Error error)
    {
        return Failure(error);
    }

    /// <summary>
    /// Matches the result and executes the appropriate function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result</typeparam>
    /// <param name="onSuccess">Function to execute on success</param>
    /// <param name="onFailure">Function to execute on failure</param>
    /// <returns>The result of the executed function</returns>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<IReadOnlyList<Error>, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value!) : onFailure(Errors);
    }
}

/// <summary>
/// Non-generic Result for operations that don't return a value.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the result represents a success.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the result represents a failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the collection of errors. Only populated when IsFailure is true.
    /// </summary>
    public IReadOnlyList<Error> Errors { get; private set; } = new List<Error>();

    /// <summary>
    /// Gets the first error or null if no errors exist.
    /// </summary>
    public Error? FirstError => Errors.FirstOrDefault();

    /// <summary>
    /// Private constructor for creating Result instances.
    /// </summary>
    private Result(bool isSuccess, IEnumerable<Error>? errors = null)
    {
        IsSuccess = isSuccess;
        Errors = errors?.ToList() ?? new List<Error>();
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful Result</returns>
    public static Result Success()
    {
        return new Result(true);
    }

    /// <summary>
    /// Creates a failed result with a single error.
    /// </summary>
    /// <param name="error">The error</param>
    /// <returns>A failed Result</returns>
    public static Result Failure(Error error)
    {
        return new Result(false, new[] { error });
    }

    /// <summary>
    /// Creates a failed result with multiple errors.
    /// </summary>
    /// <param name="errors">The collection of errors</param>
    /// <returns>A failed Result</returns>
    public static Result Failure(IEnumerable<Error> errors)
    {
        return new Result(false, errors);
    }

    /// <summary>
    /// Creates a failed result with a simple error message.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <returns>A failed Result</returns>
    public static Result Failure(string message)
    {
        return Failure(Error.General(message));
    }

    /// <summary>
    /// Implicit conversion from Error to failed Result.
    /// </summary>
    /// <param name="error">The error</param>
    public static implicit operator Result(Error error)
    {
        return Failure(error);
    }

    /// <summary>
    /// Matches the result and executes the appropriate function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result</typeparam>
    /// <param name="onSuccess">Function to execute on success</param>
    /// <param name="onFailure">Function to execute on failure</param>
    /// <returns>The result of the executed function</returns>
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<IReadOnlyList<Error>, TResult> onFailure)
    {
        return IsSuccess ? onSuccess() : onFailure(Errors);
    }
}

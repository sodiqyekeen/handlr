using System.Collections.Generic;

namespace Handlr.Abstractions.Commands;

/// <summary>
/// Marker interface for commands without return values.
/// Commands represent actions that modify state.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Gets the unique identifier for this command execution.
    /// Used for correlation, logging, and tracking.
    /// </summary>
    string? CorrelationId { get; init; }

    /// <summary>
    /// Gets additional metadata for this command.
    /// Can be used for tenant information, user context, authorization data, etc.
    /// </summary>
    IDictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Interface for commands that return a result.
/// Commands represent actions that modify state and return a value.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the command. Can be any type: User, bool, string, custom DTOs, Result&lt;T&gt;, etc.</typeparam>
public interface ICommand<out TResult> : ICommand
{
}

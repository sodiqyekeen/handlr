using System;
using System.Collections.Generic;
using Handlr.Abstractions.Commands;

namespace Handlr.Abstractions.Common;

/// <summary>
/// Base record for commands that do not return a value.
/// </summary>
public abstract record BaseCommand : ICommand
{
    /// <inheritdoc />
    public string? CorrelationId { get; init; } = Guid.NewGuid().ToString();

    /// <inheritdoc />
    public IDictionary<string, object>? Metadata { get; init; } = new Dictionary<string, object>();
}

/// <summary>
/// Base record for commands that return a value.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the command</typeparam>
public abstract record BaseCommand<TResult> : ICommand<TResult>
{
    /// <inheritdoc />
    public string? CorrelationId { get; init; } = Guid.NewGuid().ToString();

    /// <inheritdoc />
    public IDictionary<string, object>? Metadata { get; init; } = new Dictionary<string, object>();
}
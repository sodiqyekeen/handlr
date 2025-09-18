using Handlr.Abstractions.Commands;
using Handlr.Abstractions.Queries;
using Handlr.Abstractions.Results;

namespace Handlr.Tests.TestUtilities;

/// <summary>
/// Base classes for test commands and queries that implement required interface members
/// </summary>
public abstract record TestCommandBase<TResult> : ICommand<TResult>
{
    public string? CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public IDictionary<string, object>? Metadata { get; init; } = new Dictionary<string, object>();
}

public abstract record TestCommandBase : ICommand
{
    public string? CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public IDictionary<string, object>? Metadata { get; init; } = new Dictionary<string, object>();
}

public abstract record TestQueryBase<TResult> : IQuery<TResult>
{
    public string? CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public IDictionary<string, object>? Metadata { get; init; } = new Dictionary<string, object>();
}

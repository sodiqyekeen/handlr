using System;
using System.Threading;
using System.Threading.Tasks;
using Handlr.Abstractions.Commands;
using Handlr.Abstractions.Queries;
using Handlr.Abstractions.Common;

namespace Handlr.Abstractions.Extensions;

/// <summary>
/// Extension methods for IHandlrDispatcher to provide HandleAsync functionality.
/// </summary>
public static class HandlrDispatcherExtensions
{
    /// <summary>
    /// Handles a command asynchronously. This is an alias for SendAsync.
    /// </summary>
    /// <typeparam name="TCommand">The type of command</typeparam>
    /// <param name="dispatcher">The dispatcher instance</param>
    /// <param name="command">The command to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static Task HandleAsync<TCommand>(this IHandlrDispatcher dispatcher, TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));
        return dispatcher.SendAsync(command, cancellationToken);
    }

    /// <summary>
    /// Handles a command with result asynchronously. This is an alias for SendAsync.
    /// </summary>
    /// <typeparam name="TResult">The type of result</typeparam>
    /// <param name="dispatcher">The dispatcher instance</param>
    /// <param name="command">The command to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation with the result</returns>
    public static Task<TResult> HandleAsync<TResult>(this IHandlrDispatcher dispatcher, ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));
        return dispatcher.SendAsync(command, cancellationToken);
    }

    /// <summary>
    /// Handles a query asynchronously. This is an alias for SendAsync.
    /// </summary>
    /// <typeparam name="TResult">The type of result</typeparam>
    /// <param name="dispatcher">The dispatcher instance</param>
    /// <param name="query">The query to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation with the result</returns>
    public static Task<TResult> HandleAsync<TResult>(this IHandlrDispatcher dispatcher, IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));
        return dispatcher.SendAsync(query, cancellationToken);
    }
}
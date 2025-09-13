using System.Threading;
using System.Threading.Tasks;
using Handlr.Abstractions.Commands;
using Handlr.Abstractions.Queries;

namespace Handlr.Abstractions.Common;

/// <summary>
/// Interface for the Handlr dispatcher that coordinates command and query execution.
/// </summary>
public interface IHandlrDispatcher
{
    /// <summary>
    /// Sends a command for execution without expecting a return value.
    /// </summary>
    /// <typeparam name="TCommand">The type of command</typeparam>
    /// <param name="command">The command to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand;

    /// <summary>
    /// Sends a command for execution and returns a result.
    /// </summary>
    /// <typeparam name="TResult">The type of result</typeparam>
    /// <param name="command">The command to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation with the result</returns>
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a query for execution and returns a result.
    /// </summary>
    /// <typeparam name="TResult">The type of result</typeparam>
    /// <param name="query">The query to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation with the result</returns>
    Task<TResult> SendAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
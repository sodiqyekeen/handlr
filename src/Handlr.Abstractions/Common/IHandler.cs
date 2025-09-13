using System.Threading;
using System.Threading.Tasks;

namespace Handlr.Abstractions.Common;

/// <summary>
/// Common interface for all request handlers (commands and queries).
/// Useful for registering handlers in dependency injection containers.
/// </summary>
public interface IHandler
{
}

/// <summary>
/// Interface for handlers that handle a specific request type and return a result.
/// </summary>
/// <typeparam name="TRequest">The type of request to handle</typeparam>
/// <typeparam name="TResult">The type of result returned. Can be any type: User, bool, string, custom DTOs, Result&lt;T&gt;, etc.</typeparam>
public interface IHandler<in TRequest, TResult> : IHandler
{
    /// <summary>
    /// Handles the specified request and returns a result.
    /// </summary>
    /// <param name="request">The request to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation with the result</returns>
    Task<TResult> Handle(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for handlers that handle a specific request without returning a result.
/// </summary>
/// <typeparam name="TRequest">The type of request to handle</typeparam>
public interface IHandler<in TRequest> : IHandler
{
    /// <summary>
    /// Handles the specified request.
    /// </summary>
    /// <param name="request">The request to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task Handle(TRequest request, CancellationToken cancellationToken = default);
}
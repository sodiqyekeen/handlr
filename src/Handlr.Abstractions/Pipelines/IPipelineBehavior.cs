using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Handlr.Abstractions.Pipelines;

/// <summary>
/// Represents a request delegate in the pipeline.
/// </summary>
/// <typeparam name="TResult">The type of result returned. Can be any type: User, bool, string, custom DTOs, Result&lt;T&gt;, etc.</typeparam>
/// <returns>A task representing the asynchronous operation with the result</returns>
public delegate Task<TResult> RequestHandlerDelegate<TResult>();

/// <summary>
/// Base interface for pipeline behaviors that handle cross-cutting concerns.
/// Allows for validation, logging, caching, retry, metrics, authorization, and custom behaviors.
/// </summary>
/// <typeparam name="TRequest">The type of request (command or query)</typeparam>
/// <typeparam name="TResult">The type of result returned. Can be any type or void for commands without results</typeparam>
public interface IPipelineBehavior<in TRequest, TResult>
{
    /// <summary>
    /// Handles the request with the ability to execute before and after the next behavior or handler.
    /// </summary>
    /// <param name="request">The request object</param>
    /// <param name="next">The next behavior or handler in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation with the result</returns>
    Task<TResult> Handle(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for behaviors that execute conditionally based on request characteristics.
/// </summary>
/// <typeparam name="TRequest">The type of request (command or query)</typeparam>
/// <typeparam name="TResult">The type of result returned</typeparam>
public interface IConditionalBehavior<in TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
{
    /// <summary>
    /// Determines whether this behavior should execute for the given request.
    /// </summary>
    /// <param name="request">The request object</param>
    /// <returns>True if the behavior should execute; otherwise, false</returns>
    bool ShouldExecute(TRequest request);
}

/// <summary>
/// Interface for pipeline context that provides additional information and services.
/// </summary>
public interface IPipelineContext
{
    /// <summary>
    /// Gets the correlation ID for this request execution.
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// Gets the request metadata.
    /// </summary>
    IDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Gets the service provider for dependency resolution.
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the cancellation token for this request.
    /// </summary>
    CancellationToken CancellationToken { get; }
}
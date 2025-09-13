using System.Threading;
using System.Threading.Tasks;

namespace Handlr.Abstractions.Queries;

/// <summary>
/// Interface for handling queries.
/// </summary>
/// <typeparam name="TQuery">The type of query to handle</typeparam>
/// <typeparam name="TResult">The type of result returned by the query. Can be any type: User, List&lt;User&gt;, PagedResult&lt;T&gt;, custom DTOs, Result&lt;T&gt;, etc.</typeparam>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Handles the specified query and returns a result.
    /// </summary>
    /// <param name="query">The query to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation with the result</returns>
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken = default);
}
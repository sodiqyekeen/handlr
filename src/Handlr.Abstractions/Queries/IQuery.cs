using System.Collections.Generic;

namespace Handlr.Abstractions.Queries;

/// <summary>
/// Marker interface for all queries. A query is a request that retrieves data without side effects.
/// Use this interface to mark query classes for source generator detection.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the query. Can be any type: User, List&lt;User&gt;, PagedResult&lt;T&gt;, custom DTOs, Result&lt;T&gt;, etc.</typeparam>
public interface IQuery<TResult>
{
    /// <summary>
    /// Gets the unique identifier for this query execution.
    /// Used for correlation, logging, and tracking.
    /// </summary>
    string? CorrelationId { get; init; }

    /// <summary>
    /// Gets additional metadata for this query.
    /// Can be used for caching keys, tenant information, user context, etc.
    /// </summary>
    IDictionary<string, object>? Metadata { get; init; }
}

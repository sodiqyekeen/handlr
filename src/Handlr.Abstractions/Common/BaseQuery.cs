using System;
using System.Collections.Generic;
using Handlr.Abstractions.Queries;

namespace Handlr.Abstractions.Common;

/// <summary>
/// Base record for queries that provides common functionality.
/// Optional utility class - users can implement IQuery&lt;TResult&gt; directly.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the query. Can be any type: User, List&lt;User&gt;, PagedResult&lt;T&gt;, custom DTOs, Result&lt;T&gt;, etc.</typeparam>
public abstract record BaseQuery<TResult> : IQuery<TResult>
{
    /// <summary>
    /// Gets the unique identifier for this query execution.
    /// Used for correlation, logging, and tracking.
    /// </summary>
    public string? CorrelationId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets additional metadata for this query.
    /// Can be used for caching keys, tenant information, user context, etc.
    /// </summary>
    public IDictionary<string, object>? Metadata { get; init; } = new Dictionary<string, object>();
}

/// <summary>
/// Base record for paged queries that provides pagination functionality.
/// Optional utility class for queries that need pagination support.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the query</typeparam>
public abstract record PagedQuery<TResult> : BaseQuery<TResult>
{
    /// <summary>
    /// Gets or sets the page number (1-based).
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Gets or sets the page size (number of items per page).
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Gets or sets the sort field.
    /// </summary>
    public string? SortBy { get; init; }

    /// <summary>
    /// Gets or sets the sort direction (asc/desc).
    /// </summary>
    public string? SortDirection { get; init; } = "asc";
}

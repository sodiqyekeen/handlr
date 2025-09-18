#nullable enable
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Handlr.Abstractions.Commands;
using Handlr.Abstractions.Queries;
using Handlr.Abstractions.Results;

namespace SampleWebApi.Queries;

/// <summary>
/// Query that uses Data Annotations validation instead of FluentValidation
/// </summary>
public record SearchUsersQuery : IQuery<Result<List<UserDto>>>
{
    [Required(ErrorMessage = "Search term is required")]
    [MinLength(2, ErrorMessage = "Search term must be at least 2 characters")]
    [MaxLength(50, ErrorMessage = "Search term cannot exceed 50 characters")]
    public string SearchTerm { get; init; } = string.Empty;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; init; } = 10;

    [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1")]
    public int PageNumber { get; init; } = 1;

    // Required IQuery properties
    public string? CorrelationId { get; init; }
    public IDictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Handler for SearchUsersQuery using Data Annotations validation
/// </summary>
public class SearchUsersQueryHandler : IQueryHandler<SearchUsersQuery, Result<List<UserDto>>>
{
    private readonly ILogger<SearchUsersQueryHandler> _logger;

    public SearchUsersQueryHandler(ILogger<SearchUsersQueryHandler> logger)
    {
        _logger = logger;
    }

    public async Task<Result<List<UserDto>>> Handle(SearchUsersQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching users with term: {SearchTerm}, Page: {PageNumber}, Size: {PageSize}",
            query.SearchTerm, query.PageNumber, query.PageSize);

        // Simulate search logic
        await Task.Delay(100, cancellationToken);

        var searchResults = new List<UserDto>
        {
            new UserDto { Id = 1, Name = $"User matching '{query.SearchTerm}'", Email = "user@example.com", Age = 25 }
        };

        return Result<List<UserDto>>.Success(searchResults);
    }
}

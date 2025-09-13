using Handlr.Abstractions.Queries;
using Handlr.Abstractions.Common;
using Handlr.Abstractions.Results;

namespace SampleWebApi.Queries;

/// <summary>
/// Query to get user by ID
/// </summary>
public record GetUserByIdQuery : BaseQuery<Result<UserDto>>
{
    /// <summary>
    /// The ID of the user to retrieve
    /// </summary>
    public int UserId { get; init; }
}

/// <summary>
/// Query to get all users with filtering
/// </summary>
public record GetUsersQuery : BaseQuery<Result<List<UserDto>>>
{
    /// <summary>
    /// Filter users by name (contains)
    /// </summary>
    public string NameFilter { get; init; }

    /// <summary>
    /// Filter users by minimum age
    /// </summary>
    public int MinAge { get; init; }

    /// <summary>
    /// Page number for pagination
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; init; } = 10;
}

/// <summary>
/// DTO for user data
/// </summary>
public record UserDto
{
    /// <summary>
    /// User's unique identifier
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// User's name
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// User's age
    /// </summary>
    public int Age { get; init; }

    /// <summary>
    /// User's current status
    /// </summary>
    public string Status { get; init; } = string.Empty;
}
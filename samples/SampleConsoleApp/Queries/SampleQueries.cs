using Handlr.Abstractions.Common;
using Handlr.Abstractions.Queries;
using Handlr.Abstractions.Results;

namespace SampleConsoleApp.Queries;

/// <summary>
/// Query to get user by ID
/// </summary>
public record GetUserByIdQuery : BaseQuery<Result<UserDto>>
{
    public int UserId { get; init; }
}

/// <summary>
/// Query to get all users with filtering
/// </summary>
public record GetUsersQuery : BaseQuery<Result<List<UserDto>>>
{
    public string? NameFilter { get; init; }
    public int? MinAge { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

/// <summary>
/// DTO for user data
/// </summary>
public record UserDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int Age { get; init; }
    public string Status { get; init; } = string.Empty;
}

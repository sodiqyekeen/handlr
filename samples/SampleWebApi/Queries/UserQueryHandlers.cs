using Handlr.Abstractions.Results;
using SampleWebApi.Services;

namespace SampleWebApi.Queries;

/// <summary>
/// Handler for GetUserByIdQuery
/// </summary>
public partial class GetUserByIdQueryHandler
{
    /// <summary>
    /// Handles the GetUserByIdQuery
    /// </summary>
    public partial async Task<Result<UserDto>> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        // Simulate database lookup
        await Task.Delay(50, cancellationToken);

        // Validate the query
        if (query.UserId <= 0)
            return Result<UserDto>.Failure("Invalid user ID");

        // Simulate finding user
        var user = new UserDto
        {
            Id = query.UserId,
            Name = $"User {query.UserId}",
            Email = $"user{query.UserId}@example.com",
            Age = 25 + (query.UserId % 40),
            Status = "Active"
        };

        return Result<UserDto>.Success(user);
    }
}

/// <summary>
/// Handler for GetUsersQuery
/// </summary>
public partial class GetUsersQueryHandler
{
    /// <summary>
    /// Handles the GetUsersQuery
    /// </summary>
    public partial async Task<Result<List<UserDto>>> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken)
    {
        // Simulate database query
        await Task.Delay(100, cancellationToken);

        // Validate pagination
        if (query.Page <= 0)
            return Result<List<UserDto>>.Failure("Page must be greater than 0");

        if (query.PageSize <= 0 || query.PageSize > 100)
            return Result<List<UserDto>>.Failure("Page size must be between 1 and 100");

        // Simulate user data
        var users = new List<UserDto>();
        var startId = (query.Page - 1) * query.PageSize + 1;

        for (int i = 0; i < query.PageSize; i++)
        {
            var userId = startId + i;
            var user = new UserDto
            {
                Id = userId,
                Name = $"User {userId}",
                Email = $"user{userId}@example.com",
                Age = 20 + (userId % 50),
                Status = userId % 3 == 0 ? "Inactive" : "Active"
            };

            // Apply name filter if provided
            if (!string.IsNullOrWhiteSpace(query.NameFilter) &&
                !user.Name.Contains(query.NameFilter, StringComparison.OrdinalIgnoreCase))
                continue;

            // Apply age filter if provided
            if (query.MinAge > 0 && user.Age < query.MinAge)
                continue;

            users.Add(user);
        }

        return Result<List<UserDto>>.Success(users);
    }
}
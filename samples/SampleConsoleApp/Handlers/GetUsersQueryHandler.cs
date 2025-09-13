using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Handlr.Abstractions.Results;
using SampleConsoleApp.Queries;

namespace SampleConsoleApp.Queries;

/// <summary>
/// User implementation of GetUsersQuery handler
/// </summary>
public partial class GetUsersQueryHandler
{
    /// <summary>
    /// Handles the GetUsersQuery
    /// </summary>
    public partial async Task<Result<List<UserDto>>> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken)
    {
        // Simulate database query
        await Task.Delay(200, cancellationToken);

        // Generate sample users
        var allUsers = new List<UserDto>();
        for (int i = 1; i <= 100; i++)
        {
            allUsers.Add(new UserDto
            {
                Id = i,
                Name = $"User {i}",
                Email = $"user{i}@example.com",
                Age = 18 + (i % 50),
                Status = i % 3 == 0 ? "Inactive" : "Active"
            });
        }

        // Apply filters
        var filteredUsers = allUsers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.NameFilter))
        {
            filteredUsers = filteredUsers.Where(u => u.Name.Contains(query.NameFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (query.MinAge.HasValue)
        {
            filteredUsers = filteredUsers.Where(u => u.Age >= query.MinAge.Value);
        }

        // Apply pagination
        var pagedUsers = filteredUsers
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return Result<List<UserDto>>.Success(pagedUsers);
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Handlr.Abstractions.Queries;
using Handlr.Abstractions.Results;
using SampleConsoleApp.Queries;

namespace SampleConsoleApp.Queries;

/// <summary>
/// Handler for GetUserByIdQuery - no partial class required!
/// </summary>
public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, Result<UserDto>>
{
    /// <summary>
    /// Handles the GetUserByIdQuery
    /// </summary>
    public async Task<Result<UserDto>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        // Simulate database lookup
        await Task.Delay(100, cancellationToken);

        // Simulate user lookup
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

using System.Threading;
using System.Threading.Tasks;
using Handlr.Abstractions.Commands;
using Handlr.Abstractions.Results;
using SampleConsoleApp.Commands;

namespace SampleConsoleApp.Commands;

/// <summary>
/// Handler for CreateUserCommand - no partial class required!
/// </summary>
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Result<int>>
{
    /// <summary>
    /// Handles the CreateUserCommand
    /// </summary>
    public async Task<Result<int>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // Simulate creating a user
        await Task.Delay(200, cancellationToken);

        // Simulate validation
        if (string.IsNullOrWhiteSpace(command.Name))
            return Result<int>.Failure("Name is required");

        if (string.IsNullOrWhiteSpace(command.Email))
            return Result<int>.Failure("Email is required");

        // Simulate user creation and return the new user ID
        var newUserId = Random.Shared.Next(1000, 9999);
        return Result<int>.Success(newUserId);
    }
}

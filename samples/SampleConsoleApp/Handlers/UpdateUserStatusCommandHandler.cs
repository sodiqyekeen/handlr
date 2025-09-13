using System.Threading;
using System.Threading.Tasks;
using SampleConsoleApp.Commands;

namespace SampleConsoleApp.Commands;

/// <summary>
/// User implementation of UpdateUserStatusCommand handler
/// </summary>
public partial class UpdateUserStatusCommandHandler
{
    /// <summary>
    /// Handles the UpdateUserStatusCommand
    /// </summary>
    public partial async Task HandleAsync(UpdateUserStatusCommand command, CancellationToken cancellationToken)
    {
        // Simulate updating user status
        await Task.Delay(150, cancellationToken);

        // Log the status update
        Console.WriteLine($"Updated user {command.UserId} status to: {command.Status}");
    }
}
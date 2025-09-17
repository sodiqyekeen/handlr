using System;
using System.Threading;
using System.Threading.Tasks;
using Handlr.Abstractions.Commands;
using SampleConsoleApp.Commands;

namespace SampleConsoleApp.Commands;

/// <summary>
/// Handler for UpdateUserStatusCommand - no partial class required!
/// </summary>
public class UpdateUserStatusCommandHandler : ICommandHandler<UpdateUserStatusCommand>
{
    /// <summary>
    /// Handles the UpdateUserStatusCommand
    /// </summary>
    public async Task Handle(UpdateUserStatusCommand command, CancellationToken cancellationToken)
    {
        // Simulate updating user status
        await Task.Delay(150, cancellationToken);

        // Log the status update
        Console.WriteLine($"Updated user {command.UserId} status to: {command.Status}");
    }
}

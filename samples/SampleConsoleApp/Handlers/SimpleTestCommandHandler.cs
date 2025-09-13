using Handlr.Abstractions.Results;

namespace SampleConsoleApp.Commands;

/// <summary>
/// User implementation of SimpleTestCommand handler
/// </summary>
public partial class SimpleTestCommandHandler
{
    /// <summary>
    /// Handles the SimpleTestCommand
    /// </summary>
    public partial async Task<Result<string>> HandleAsync(SimpleTestCommand command, CancellationToken cancellationToken)
    {
        // Simulate some work
        await Task.Delay(100, cancellationToken);

        return Result<string>.Success($"Processed test command with value: {command.TestValue}");
    }
}
using System.Threading;
using System.Threading.Tasks;
using Handlr.Abstractions.Commands;
using Handlr.Abstractions.Results;

namespace SampleConsoleApp.Commands;

/// <summary>
/// Handler for SimpleTestCommand - no partial class required!
/// </summary>
public class SimpleTestCommandHandler : ICommandHandler<SimpleTestCommand, Result<string>>
{
    /// <summary>
    /// Handles the SimpleTestCommand
    /// </summary>
    public async Task<Result<string>> Handle(SimpleTestCommand command, CancellationToken cancellationToken)
    {
        // Simulate some work
        await Task.Delay(100, cancellationToken);

        return Result<string>.Success($"Processed test command with value: {command.TestValue}");
    }
}

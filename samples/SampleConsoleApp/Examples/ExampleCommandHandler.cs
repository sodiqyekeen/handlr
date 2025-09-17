using System.Threading;
using System.Threading.Tasks;
using Handlr.Abstractions.Commands;
using Handlr.Abstractions.Results;

namespace SampleConsoleApp.Examples;

/// <summary>
/// Example handler for ExampleCommand - no partial class required!
/// </summary>
public class ExampleCommandHandler : ICommandHandler<ExampleCommand, Result<string>>
{
    /// <summary>
    /// Handles the ExampleCommand
    /// </summary>
    public async Task<Result<string>> Handle(ExampleCommand command, CancellationToken cancellationToken)
    {
        // Simulate some processing
        await Task.Delay(100, cancellationToken);

        return Result<string>.Success($"Processed command for {command.Name} (age {command.Age})");
    }
}

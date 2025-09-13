using System.Threading;
using System.Threading.Tasks;
using Handlr.Abstractions.Results;

namespace SampleConsoleApp.Examples;

/// <summary>
/// Example handler for ExampleCommand - shows how the source generator will work
/// This is the user implementation part of the partial class
/// </summary>
public partial class ExampleCommandHandler
{
    /// <summary>
    /// Implements the partial method defined by the source generator
    /// </summary>
    public partial async Task<Result<string>> HandleAsync(ExampleCommand command, CancellationToken cancellationToken)
    {
        // Simulate some processing
        await Task.Delay(100, cancellationToken);

        return Result<string>.Success($"Processed command for {command.Name} (age {command.Age})");
    }
}
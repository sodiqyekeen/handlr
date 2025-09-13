using System.Threading;
using System.Threading.Tasks;
using Handlr.Abstractions.Results;

namespace SampleConsoleApp.Examples;

/// <summary>
/// Example handler for ExampleQuery - shows how the source generator will work
/// This is the user implementation part of the partial class
/// </summary>
public partial class ExampleQueryHandler
{
    /// <summary>
    /// Implements the partial method defined by the source generator
    /// </summary>
    public partial async Task<Result<string>> HandleAsync(ExampleQuery query, CancellationToken cancellationToken)
    {
        // Simulate some processing
        await Task.Delay(200, cancellationToken);

        return Result<string>.Success($"User data for {query.UserId}");
    }
}
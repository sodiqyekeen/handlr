using System.Threading;
using System.Threading.Tasks;
using Handlr.Abstractions.Queries;
using Handlr.Abstractions.Results;

namespace SampleConsoleApp.Examples;

/// <summary>
/// Example handler for ExampleQuery - no partial class required!
/// </summary>
public class ExampleQueryHandler : IQueryHandler<ExampleQuery, Result<string>>
{
    /// <summary>
    /// Handles the ExampleQuery
    /// </summary>
    public async Task<Result<string>> Handle(ExampleQuery query, CancellationToken cancellationToken)
    {
        // Simulate some processing
        await Task.Delay(200, cancellationToken);

        return Result<string>.Success($"User data for {query.UserId}");
    }
}

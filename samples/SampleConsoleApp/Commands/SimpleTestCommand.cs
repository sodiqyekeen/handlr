using System.Collections.Generic;
using Handlr.Abstractions.Commands;
using Handlr.Abstractions.Results;

namespace SampleConsoleApp.Commands;

/// <summary>
/// Simple test command that directly implements ICommand
/// </summary>
public record SimpleTestCommand : ICommand<Result<string>>
{
    public string? CorrelationId { get; init; }
    public IDictionary<string, object>? Metadata { get; init; }
    public string TestValue { get; init; } = string.Empty;
}

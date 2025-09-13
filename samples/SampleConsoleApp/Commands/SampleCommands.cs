using Handlr.Abstractions.Commands;
using Handlr.Abstractions.Common;
using Handlr.Abstractions.Queries;
using Handlr.Abstractions.Results;

namespace SampleConsoleApp.Commands;

/// <summary>
/// Command to create a new user
/// </summary>
public record CreateUserCommand : BaseCommand<Result<int>>
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int Age { get; init; }
}

/// <summary>
/// Simple command without return type
/// </summary>
public record UpdateUserStatusCommand : BaseCommand
{
    public int UserId { get; init; }
    public string Status { get; init; } = string.Empty;
}

/// <summary>
/// Command with complex return type
/// </summary>
public record GenerateReportCommand : BaseCommand<Result<string>>
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string ReportType { get; init; } = string.Empty;
}

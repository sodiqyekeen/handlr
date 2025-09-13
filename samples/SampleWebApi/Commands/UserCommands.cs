using Handlr.Abstractions.Commands;
using Handlr.Abstractions.Common;
using Handlr.Abstractions.Results;

namespace SampleWebApi.Commands;

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
/// Command to update user status
/// </summary>
public record UpdateUserStatusCommand : BaseCommand<Result>
{
    public int UserId { get; init; }
    public string Status { get; init; } = string.Empty;
}

/// <summary>
/// Command to generate a report
/// </summary>
public record GenerateReportCommand : BaseCommand<Result<string>>
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string ReportType { get; init; } = string.Empty;
}
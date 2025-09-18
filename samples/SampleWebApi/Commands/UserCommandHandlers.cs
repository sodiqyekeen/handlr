using System;
using System.Threading;
using System.Threading.Tasks;
using Handlr.Abstractions.Commands;
using Handlr.Abstractions.Results;
using SampleWebApi.Services;

namespace SampleWebApi.Commands;

/// <summary>
/// Handler for CreateUserCommand - no partial class required!
/// </summary>
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Result<int>>
{
    /// <summary>
    /// Handles the CreateUserCommand
    /// </summary>
    public async Task<Result<int>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // NOTE: Validation is now handled by ValidationBehavior in the pipeline
        // This handler focuses only on business logic

        // Simulate creating a user
        await Task.Delay(100, cancellationToken);

        // Simulate user creation and return the new user ID
        var newUserId = Random.Shared.Next(1000, 9999);

        return Result<int>.Success(newUserId);
    }
}

/// <summary>
/// Handler for UpdateUserStatusCommand - no partial class required!
/// </summary>
public class UpdateUserStatusCommandHandler : ICommandHandler<UpdateUserStatusCommand, Result>
{
    /// <summary>
    /// Handles the UpdateUserStatusCommand
    /// </summary>
    public async Task<Result> Handle(UpdateUserStatusCommand command, CancellationToken cancellationToken)
    {
        // NOTE: Validation is now handled by ValidationBehavior in the pipeline
        // This handler focuses only on business logic

        // Simulate updating user status
        await Task.Delay(50, cancellationToken);

        // Simulate status update
        return Result.Success();
    }
}

/// <summary>
/// Handler for GenerateReportCommand - no partial class required!
/// </summary>
public class GenerateReportCommandHandler : ICommandHandler<GenerateReportCommand, Result<string>>
{
    /// <summary>
    /// Handles the GenerateReportCommand
    /// </summary>
    public async Task<Result<string>> Handle(GenerateReportCommand command, CancellationToken cancellationToken)
    {
        // NOTE: Validation is now handled by ValidationBehavior in the pipeline
        // This handler focuses only on business logic

        // Simulate report generation
        await Task.Delay(200, cancellationToken);

        // Simulate report generation
        var userCount = Random.Shared.Next(50, 500);

        var reportData = command.ReportType.ToLower() switch
        {
            "summary" => $"Summary Report: {userCount} total users between {command.StartDate:yyyy-MM-dd} and {command.EndDate:yyyy-MM-dd}",
            "detailed" => $"Detailed Report: Found {userCount} users. Average age: {Random.Shared.Next(25, 45):F1} years",
            "analytics" => $"Analytics Report: {userCount} users analyzed with {Random.Shared.Next(5, 15)} key insights generated",
            _ => $"Standard Report: {userCount} users found"
        };

        return Result<string>.Success(reportData);
    }
}

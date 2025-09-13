using Handlr.Abstractions.Results;
using SampleWebApi.Services;

namespace SampleWebApi.Commands;

/// <summary>
/// Handler for CreateUserCommand
/// </summary>
public partial class CreateUserCommandHandler
{
    /// <summary>
    /// Handles the CreateUserCommand
    /// </summary>
    public partial async Task<Result<int>> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // Simulate creating a user
        await Task.Delay(100, cancellationToken);

        // Validate the command
        if (string.IsNullOrWhiteSpace(command.Name))
            return Result<int>.Failure("Name is required");

        if (string.IsNullOrWhiteSpace(command.Email))
            return Result<int>.Failure("Email is required");

        if (command.Age < 0 || command.Age > 150)
            return Result<int>.Failure("Age must be between 0 and 150");

        // Simulate user creation and return the new user ID
        var newUserId = Random.Shared.Next(1000, 9999);

        return Result<int>.Success(newUserId);
    }
}

/// <summary>
/// Handler for UpdateUserStatusCommand
/// </summary>
public partial class UpdateUserStatusCommandHandler
{
    /// <summary>
    /// Handles the UpdateUserStatusCommand
    /// </summary>
    public partial async Task<Result> HandleAsync(UpdateUserStatusCommand command, CancellationToken cancellationToken)
    {
        // Simulate updating user status
        await Task.Delay(50, cancellationToken);

        // Validate the command
        if (command.UserId <= 0)
            return Result.Failure("Invalid user ID");

        if (string.IsNullOrWhiteSpace(command.Status))
            return Result.Failure("Status is required");

        // Simulate status update
        return Result.Success();
    }
}

/// <summary>
/// Handler for GenerateReportCommand
/// </summary>
public partial class GenerateReportCommandHandler
{
    /// <summary>
    /// Handles the GenerateReportCommand
    /// </summary>
    public partial async Task<Result<string>> HandleAsync(GenerateReportCommand command, CancellationToken cancellationToken)
    {
        // Simulate report generation
        await Task.Delay(200, cancellationToken);

        // Validate the command
        if (command.EndDate < command.StartDate)
            return Result<string>.Failure("End date must be after start date");

        if (string.IsNullOrWhiteSpace(command.ReportType))
            return Result<string>.Failure("Report type is required");

        // Simulate report generation
        var userCount = Random.Shared.Next(50, 500);

        var reportData = command.ReportType.ToLower() switch
        {
            "summary" => $"Summary Report: {userCount} total users between {command.StartDate:yyyy-MM-dd} and {command.EndDate:yyyy-MM-dd}",
            "detailed" => $"Detailed Report: Found {userCount} users. Average age: {Random.Shared.Next(25, 45):F1} years",
            _ => $"Standard Report: {userCount} users found"
        };

        return Result<string>.Success(reportData);
    }
}
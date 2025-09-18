using FluentValidation;

namespace SampleWebApi.Commands;

/// <summary>
/// FluentValidation validator for CreateUserCommand
/// This will be discovered and executed by the ValidationBehavior in the pipeline
/// </summary>
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .Length(2, 100)
            .WithMessage("Name must be between 2 and 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address");

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Age must be 0 or greater")
            .LessThanOrEqualTo(150)
            .WithMessage("Age must be 150 or less");
    }
}

/// <summary>
/// FluentValidation validator for UpdateUserStatusCommand
/// </summary>
public class UpdateUserStatusCommandValidator : AbstractValidator<UpdateUserStatusCommand>
{
    public UpdateUserStatusCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0");

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required")
            .Must(BeValidStatus)
            .WithMessage("Status must be one of: Active, Inactive, Suspended, Deleted");
    }

    private static bool BeValidStatus(string status)
    {
        var validStatuses = new[] { "Active", "Inactive", "Suspended", "Deleted" };
        return validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
    }
}

/// <summary>
/// FluentValidation validator for GenerateReportCommand
/// </summary>
public class GenerateReportCommandValidator : AbstractValidator<GenerateReportCommand>
{
    public GenerateReportCommandValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required")
            .LessThanOrEqualTo(x => x.EndDate)
            .WithMessage("Start date must be before or equal to end date");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("End date is required")
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be after or equal to start date");

        RuleFor(x => x.ReportType)
            .NotEmpty()
            .WithMessage("Report type is required")
            .Must(BeValidReportType)
            .WithMessage("Report type must be one of: summary, detailed, analytics");
    }

    private static bool BeValidReportType(string reportType)
    {
        var validTypes = new[] { "summary", "detailed", "analytics" };
        return validTypes.Contains(reportType, StringComparer.OrdinalIgnoreCase);
    }
}

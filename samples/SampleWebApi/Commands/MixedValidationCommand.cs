#nullable enable
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Handlr.Abstractions.Commands;
using Handlr.Abstractions.Results;

namespace SampleWebApi.Commands;

/// <summary>
/// Command that uses both FluentValidation AND Data Annotations for demonstration
/// </summary>
public record CreateUserWithMixedValidationCommand : ICommand<Result<int>>
{
    // Data Annotations validation
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; init; } = string.Empty;

    [Range(0, 150, ErrorMessage = "Age must be between 0 and 150")]
    public int Age { get; init; }

    // Required ICommand properties
    public string? CorrelationId { get; init; }
    public IDictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// FluentValidation validator for the mixed validation command
/// This will work alongside Data Annotations
/// </summary>
public class CreateUserWithMixedValidationCommandValidator : AbstractValidator<CreateUserWithMixedValidationCommand>
{
    public CreateUserWithMixedValidationCommandValidator()
    {
        // Additional FluentValidation rules that complement Data Annotations
        RuleFor(x => x.Email)
            .Must(email => !email.Contains("spam"))
            .WithMessage("Email cannot contain 'spam'");

        RuleFor(x => x.Name)
            .Must(name => !name.Contains("admin"))
            .WithMessage("Name cannot contain 'admin'");

        RuleFor(x => x.Age)
            .Must(age => age != 13)
            .WithMessage("Age 13 is not allowed for business reasons");
    }
}

/// <summary>
/// Handler for mixed validation command
/// </summary>
public class CreateUserWithMixedValidationCommandHandler : ICommandHandler<CreateUserWithMixedValidationCommand, Result<int>>
{
    private readonly ILogger<CreateUserWithMixedValidationCommandHandler> _logger;

    public CreateUserWithMixedValidationCommandHandler(ILogger<CreateUserWithMixedValidationCommandHandler> logger)
    {
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateUserWithMixedValidationCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating user with mixed validation: {Name}, {Email}, {Age}",
            command.Name, command.Email, command.Age);

        // Simulate user creation
        await Task.Delay(100, cancellationToken);

        var userId = Random.Shared.Next(1000, 9999);
        return Result<int>.Success(userId);
    }
}

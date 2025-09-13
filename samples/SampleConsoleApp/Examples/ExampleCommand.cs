using System;
using System.Collections.Generic;
using Handlr.Abstractions.Commands;
using Handlr.Abstractions.Results;
using SampleConsoleApp.Examples;

namespace SampleConsoleApp.Examples;

/// <summary>
/// Example command that shows how to implement validation and other behavior interfaces
/// </summary>
public record ExampleCommand : ICommand<Result<string>>, IValidatable
{
    public string? Name { get; init; }
    public int Age { get; init; }
    public string? Email { get; init; }

    // Required ICommand properties
    public string? CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public IDictionary<string, object>? Metadata { get; init; } = new Dictionary<string, object>();

    // Implement IValidatable to work with ValidationBehaviorExample
    public ValidationResult Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Name is required");

        if (Age < 0 || Age > 150)
            errors.Add("Age must be between 0 and 150");

        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
            errors.Add("Valid email is required");

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors.ToArray());
    }
}

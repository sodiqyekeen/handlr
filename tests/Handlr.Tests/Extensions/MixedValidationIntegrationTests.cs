using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using FluentValidation;
using Handlr.Abstractions.Commands;
using Handlr.Abstractions.Pipelines;
using Handlr.Abstractions.Queries;
using Handlr.Abstractions.Results;
using Handlr.Extensions.DataAnnotations;
using Handlr.Extensions.FluentValidation;
using Handlr.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Handlr.Tests.Extensions;

/// <summary>
/// Integration tests for mixed validation scenarios using both FluentValidation and DataAnnotations
/// </summary>
public class MixedValidationIntegrationTests
{
    #region Test Commands

    public record MixedValidationCommand : TestCommandBase<Result<string>>
    {
        [Required(ErrorMessage = "Name is required by DataAnnotations")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name length validation from DataAnnotations")]
        public string Name { get; init; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email format validation from DataAnnotations")]
        public string Email { get; init; } = string.Empty;

        [Range(18, 120, ErrorMessage = "Age range validation from DataAnnotations")]
        public int Age { get; init; }

        // This field will only be validated by FluentValidation
        public string PhoneNumber { get; init; } = string.Empty;
    }

    public record DataAnnotationsOnlyCommand : TestCommandBase<Result<string>>
    {
        [Required]
        [StringLength(100)]
        public string Data { get; init; } = string.Empty;
    }

    public record FluentValidationOnlyCommand : TestCommandBase<Result<string>>
    {
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
    }

    public record ConflictingValidationCommand : TestCommandBase<Result<string>>
    {
        [StringLength(10, ErrorMessage = "DataAnnotations: Max 10 characters")]
        public string Value { get; init; } = string.Empty;
    }

    #endregion

    #region Validators

    public class MixedValidationCommandValidator : AbstractValidator<MixedValidationCommand>
    {
        public MixedValidationCommandValidator()
        {
            // FluentValidation rules that complement DataAnnotations
            RuleFor(x => x.Name)
                .Must(NotContainNumbers)
                .WithMessage("Name must not contain numbers (FluentValidation rule)");

            RuleFor(x => x.Email)
                .Must(BeFromAllowedDomain)
                .WithMessage("Email must be from allowed domain (FluentValidation rule)");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage("Phone number is required (FluentValidation only)")
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .WithMessage("Phone number format is invalid (FluentValidation only)");
        }

        private static bool NotContainNumbers(string name)
        {
            return !name.Any(char.IsDigit);
        }

        private static bool BeFromAllowedDomain(string email)
        {
            return string.IsNullOrEmpty(email) || email.EndsWith("@example.com") || email.EndsWith("@test.com");
        }
    }

    public class FluentValidationOnlyCommandValidator : AbstractValidator<FluentValidationOnlyCommand>
    {
        public FluentValidationOnlyCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required by FluentValidation");

            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("Email format validation by FluentValidation");
        }
    }

    public class ConflictingValidationCommandValidator : AbstractValidator<ConflictingValidationCommand>
    {
        public ConflictingValidationCommandValidator()
        {
            RuleFor(x => x.Value)
                .MinimumLength(15)
                .WithMessage("FluentValidation: Min 15 characters");
        }
    }

    #endregion

    #region Handlers

    public class MixedValidationCommandHandler : ICommandHandler<MixedValidationCommand, Result<string>>
    {
        public Task<Result<string>> Handle(MixedValidationCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success($"Processed: {command.Name}"));
        }
    }

    public class DataAnnotationsOnlyCommandHandler : ICommandHandler<DataAnnotationsOnlyCommand, Result<string>>
    {
        public Task<Result<string>> Handle(DataAnnotationsOnlyCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success($"Processed: {command.Data}"));
        }
    }

    public class FluentValidationOnlyCommandHandler : ICommandHandler<FluentValidationOnlyCommand, Result<string>>
    {
        public Task<Result<string>> Handle(FluentValidationOnlyCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success($"Processed: {command.Name}"));
        }
    }

    public class ConflictingValidationCommandHandler : ICommandHandler<ConflictingValidationCommand, Result<string>>
    {
        public Task<Result<string>> Handle(ConflictingValidationCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success($"Processed: {command.Value}"));
        }
    }

    #endregion

    [Fact]
    public void BothValidationExtensions_ShouldRegisterCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHandlrFluentValidation();
        services.AddHandlrDataAnnotations();
        services.AddScoped<IValidator<MixedValidationCommand>, MixedValidationCommandValidator>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<MixedValidationCommand, Result<string>>>();

        behaviors.Should().HaveCount(2);
        behaviors.Should().Contain(b => b.GetType().Name.Contains("FluentValidation"));
        behaviors.Should().Contain(b => b.GetType().Name.Contains("DataAnnotations"));
    }

    [Fact]
    public async Task MixedValidation_WithBothValidationTypes_ShouldValidateAll()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrFluentValidation();
        services.AddHandlrDataAnnotations();
        services.AddScoped<IValidator<MixedValidationCommand>, MixedValidationCommandValidator>();

        var serviceProvider = services.BuildServiceProvider();
        var fluentBehavior = serviceProvider.GetServices<IPipelineBehavior<MixedValidationCommand, Result<string>>>()
            .First(b => b.GetType().Name.Contains("FluentValidation"));
        var dataBehavior = serviceProvider.GetServices<IPipelineBehavior<MixedValidationCommand, Result<string>>>()
            .First(b => b.GetType().Name.Contains("DataAnnotations"));

        var invalidCommand = new MixedValidationCommand
        {
            Name = "John123", // Invalid: contains numbers (FluentValidation) and valid for DataAnnotations
            Email = "john@invalid.com", // Invalid: not from allowed domain (FluentValidation) but valid format (DataAnnotations)
            Age = 15, // Invalid: under 18 (DataAnnotations)
            PhoneNumber = "" // Invalid: empty (FluentValidation only)
        };

        RequestHandlerDelegate<Result<string>> next = () => Task.FromResult(Result<string>.Success("Success"));

        // Act & Assert - DataAnnotations should run first
        var dataException = await Assert.ThrowsAsync<DataAnnotationsValidationException>(
            () => dataBehavior.Handle(invalidCommand, next, CancellationToken.None));

        dataException.ValidationResults.Should().Contain(e => e.MemberNames.Contains(nameof(MixedValidationCommand.Age)));

        // FluentValidation would also fail if it ran
        var fluentException = await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => fluentBehavior.Handle(invalidCommand, next, CancellationToken.None));

        fluentException.Errors.Should().Contain(e => e.PropertyName == nameof(MixedValidationCommand.Name));
        fluentException.Errors.Should().Contain(e => e.PropertyName == nameof(MixedValidationCommand.Email));
        fluentException.Errors.Should().Contain(e => e.PropertyName == nameof(MixedValidationCommand.PhoneNumber));
    }

    [Fact]
    public async Task MixedValidation_WithValidCommand_ShouldPassAllValidations()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrFluentValidation();
        services.AddHandlrDataAnnotations();
        services.AddScoped<IValidator<MixedValidationCommand>, MixedValidationCommandValidator>();

        var serviceProvider = services.BuildServiceProvider();
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<MixedValidationCommand, Result<string>>>();

        var validCommand = new MixedValidationCommand
        {
            Name = "John Doe", // Valid for both
            Email = "john@example.com", // Valid for both
            Age = 25, // Valid for DataAnnotations
            PhoneNumber = "+1234567890" // Valid for FluentValidation
        };

        var nextCallCount = 0;
        RequestHandlerDelegate<Result<string>> next = () =>
        {
            nextCallCount++;
            return Task.FromResult(Result<string>.Success("Success"));
        };

        // Act - Run both behaviors in sequence
        var result = await behaviors.First().Handle(validCommand,
            () => behaviors.Last().Handle(validCommand, next, CancellationToken.None),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        nextCallCount.Should().Be(1);
    }

    [Fact]
    public async Task DataAnnotationsOnly_WithOnlyDataAnnotationsBehavior_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrDataAnnotations(); // Only DataAnnotations

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<DataAnnotationsOnlyCommand, Result<string>>>();

        var invalidCommand = new DataAnnotationsOnlyCommand
        {
            Data = "" // Invalid - required
        };

        RequestHandlerDelegate<Result<string>> next = () => Task.FromResult(Result<string>.Success("Success"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DataAnnotationsValidationException>(
            () => behavior.Handle(invalidCommand, next, CancellationToken.None));

        exception.ValidationResults.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task FluentValidationOnly_WithOnlyFluentValidationBehavior_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrFluentValidation(); // Only FluentValidation
        services.AddScoped<IValidator<FluentValidationOnlyCommand>, FluentValidationOnlyCommandValidator>();

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<FluentValidationOnlyCommand, Result<string>>>();

        var invalidCommand = new FluentValidationOnlyCommand
        {
            Name = "", // Invalid - required
            Email = "invalid-email" // Invalid - format
        };

        RequestHandlerDelegate<Result<string>> next = () => Task.FromResult(Result<string>.Success("Success"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => behavior.Handle(invalidCommand, next, CancellationToken.None));

        exception.Errors.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task ConflictingValidationRules_ShouldBothRun()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrFluentValidation();
        services.AddHandlrDataAnnotations();
        services.AddScoped<IValidator<ConflictingValidationCommand>, ConflictingValidationCommandValidator>();

        var serviceProvider = services.BuildServiceProvider();
        var fluentBehavior = serviceProvider.GetServices<IPipelineBehavior<ConflictingValidationCommand, Result<string>>>()
            .First(b => b.GetType().Name.Contains("FluentValidation"));
        var dataBehavior = serviceProvider.GetServices<IPipelineBehavior<ConflictingValidationCommand, Result<string>>>()
            .First(b => b.GetType().Name.Contains("DataAnnotations"));

        var conflictingCommand = new ConflictingValidationCommand
        {
            Value = "12345678901234" // 14 characters - fails DataAnnotations (max 10) but would pass FluentValidation (min 15)
        };

        RequestHandlerDelegate<Result<string>> next = () => Task.FromResult(Result<string>.Success("Success"));

        // Act & Assert - DataAnnotations should fail (max 10)
        var dataException = await Assert.ThrowsAsync<DataAnnotationsValidationException>(
            () => dataBehavior.Handle(conflictingCommand, next, CancellationToken.None));

        dataException.ValidationResults.Should().Contain(e => e.ErrorMessage != null && e.ErrorMessage.Contains("Max 10 characters"));

        // FluentValidation should also fail (min 15)
        var fluentException = await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => fluentBehavior.Handle(conflictingCommand, next, CancellationToken.None));

        fluentException.Errors.Should().Contain(e => e.ErrorMessage.Contains("Min 15 characters"));
    }

    [Fact]
    public void ValidationOrder_ShouldDependOnRegistrationOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Register DataAnnotations first, then FluentValidation
        services.AddHandlrDataAnnotations();
        services.AddHandlrFluentValidation();
        services.AddScoped<IValidator<MixedValidationCommand>, MixedValidationCommandValidator>();

        var serviceProvider = services.BuildServiceProvider();
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<MixedValidationCommand, Result<string>>>().ToList();

        // Assert
        behaviors.Should().HaveCount(2);
        // The order should reflect registration order
        behaviors[0].GetType().Name.Should().Contain("DataAnnotations");
        behaviors[1].GetType().Name.Should().Contain("FluentValidation");
    }

    [Fact]
    public async Task ComplementaryValidation_ShouldAllowDifferentRulesPerFramework()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrFluentValidation();
        services.AddHandlrDataAnnotations();
        services.AddScoped<IValidator<MixedValidationCommand>, MixedValidationCommandValidator>();

        var serviceProvider = services.BuildServiceProvider();
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<MixedValidationCommand, Result<string>>>();

        var commandWithComplementaryIssues = new MixedValidationCommand
        {
            Name = "John Doe", // Valid for DataAnnotations (length), valid for FluentValidation (no numbers)
            Email = "john@example.com", // Valid for both
            Age = 25, // Valid for DataAnnotations
            PhoneNumber = "+1234567890" // Only validated by FluentValidation
        };

        var nextCallCount = 0;
        RequestHandlerDelegate<Result<string>> next = () =>
        {
            nextCallCount++;
            return Task.FromResult(Result<string>.Success("Success"));
        };

        // Act
        var result = await behaviors.First().Handle(commandWithComplementaryIssues,
            () => behaviors.Last().Handle(commandWithComplementaryIssues, next, CancellationToken.None),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        nextCallCount.Should().Be(1);
    }

    [Fact]
    public void ExceptionTypes_ShouldBeDifferentiable()
    {
        // Arrange & Act
        var fluentException = new FluentValidation.ValidationException("Fluent validation failed");
        var dataException = new DataAnnotationsValidationException(new List<ValidationResult>
        {
            new("Test error", new[] { "TestProperty" })
        });

        // Assert
        fluentException.Should().BeOfType<FluentValidation.ValidationException>();
        dataException.Should().BeOfType<DataAnnotationsValidationException>();

        // They should be different types for proper error handling
        fluentException.Should().NotBeOfType<DataAnnotationsValidationException>();
        dataException.Should().NotBeOfType<FluentValidation.ValidationException>();
    }
}

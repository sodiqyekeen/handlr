using FluentAssertions;
using FluentValidation;
using Handlr.Abstractions.Commands;
using Handlr.Abstractions.Pipelines;
using Handlr.Abstractions.Queries;
using Handlr.Abstractions.Results;
using Handlr.Extensions.FluentValidation;
using Handlr.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Handlr.Tests.Extensions;

/// <summary>
/// Comprehensive tests for FluentValidation extension functionality
/// </summary>
public class FluentValidationExtensionTests
{
    #region Test Commands and Queries

    public record TestValidationCommand : TestCommandBase<Result<string>>
    {
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public int Age { get; init; }
    }

    public record TestValidationQuery : TestQueryBase<Result<string>>
    {
        public string SearchTerm { get; init; } = string.Empty;
        public int PageSize { get; init; } = 10;
    }

    public record TestNoValidationCommand : TestCommandBase<Result<string>>
    {
        public string Data { get; init; } = string.Empty;
    }

    #endregion

    #region Validators

    public class TestValidationCommandValidator : AbstractValidator<TestValidationCommand>
    {
        public TestValidationCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required")
                .Length(2, 50)
                .WithMessage("Name must be between 2 and 50 characters");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Email must be valid");

            RuleFor(x => x.Age)
                .GreaterThanOrEqualTo(18)
                .WithMessage("Age must be at least 18")
                .LessThanOrEqualTo(120)
                .WithMessage("Age must be realistic");
        }
    }

    public class TestValidationQueryValidator : AbstractValidator<TestValidationQuery>
    {
        public TestValidationQueryValidator()
        {
            RuleFor(x => x.SearchTerm)
                .NotEmpty()
                .WithMessage("Search term is required")
                .MinimumLength(2)
                .WithMessage("Search term must be at least 2 characters");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("Page size must be positive")
                .LessThanOrEqualTo(100)
                .WithMessage("Page size cannot exceed 100");
        }
    }

    #endregion

    #region Handlers

    public class TestValidationCommandHandler : ICommandHandler<TestValidationCommand, Result<string>>
    {
        public Task<Result<string>> Handle(TestValidationCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success($"Created user: {command.Name}"));
        }
    }

    public class TestValidationQueryHandler : IQueryHandler<TestValidationQuery, Result<string>>
    {
        public Task<Result<string>> Handle(TestValidationQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success($"Search results for: {query.SearchTerm}"));
        }
    }

    public class TestNoValidationCommandHandler : ICommandHandler<TestNoValidationCommand, Result<string>>
    {
        public Task<Result<string>> Handle(TestNoValidationCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success($"Processed: {command.Data}"));
        }
    }

    #endregion

    [Fact]
    public void AddHandlrFluentValidation_ShouldRegisterBehavior()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IValidator<TestValidationCommand>, TestValidationCommandValidator>();

        // Act
        services.AddHandlrFluentValidation();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TestValidationCommand, Result<string>>>();

        behaviors.Should().NotBeEmpty();
        behaviors.Should().ContainSingle(b => b.GetType().Name.Contains("FluentValidation"));
    }

    [Fact]
    public async Task FluentValidationBehavior_WithValidRequest_ShouldCallNext()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrFluentValidation();
        services.AddScoped<IValidator<TestValidationCommand>, TestValidationCommandValidator>();

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<TestValidationCommand, Result<string>>>();

        var validCommand = new TestValidationCommand
        {
            Name = "John Doe",
            Email = "john@example.com",
            Age = 25
        };

        var nextCalled = false;
        RequestHandlerDelegate<Result<string>> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result<string>.Success("Success"));
        };

        // Act
        var result = await behavior.Handle(validCommand, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task FluentValidationBehavior_WithInvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrFluentValidation();
        services.AddScoped<IValidator<TestValidationCommand>, TestValidationCommandValidator>();

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<TestValidationCommand, Result<string>>>();

        var invalidCommand = new TestValidationCommand
        {
            Name = "", // Invalid - empty
            Email = "invalid-email", // Invalid - not email format
            Age = 15 // Invalid - under 18
        };

        RequestHandlerDelegate<Result<string>> next = () => Task.FromResult(Result<string>.Success("Success"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(invalidCommand, next, CancellationToken.None));

        exception.Errors.Should().HaveCountGreaterThan(0);
        exception.Errors.Should().Contain(e => e.PropertyName == nameof(TestValidationCommand.Name));
        exception.Errors.Should().Contain(e => e.PropertyName == nameof(TestValidationCommand.Email));
        exception.Errors.Should().Contain(e => e.PropertyName == nameof(TestValidationCommand.Age));
    }

    [Fact]
    public async Task FluentValidationBehavior_WithNoValidator_ShouldCallNext()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrFluentValidation();
        // Note: No validator registered for TestNoValidationCommand

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<TestNoValidationCommand, Result<string>>>();

        var command = new TestNoValidationCommand { Data = "test" };

        var nextCalled = false;
        RequestHandlerDelegate<Result<string>> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result<string>.Success("Success"));
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task FluentValidationBehavior_WithMultipleValidators_ShouldRunAllValidators()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrFluentValidation();
        services.AddScoped<IValidator<TestValidationCommand>, TestValidationCommandValidator>();
        services.AddScoped<IValidator<TestValidationCommand>, AdditionalTestValidator>();

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<TestValidationCommand, Result<string>>>();

        var invalidCommand = new TestValidationCommand
        {
            Name = "John Doe",
            Email = "john@example.com",
            Age = 25 // This will fail the additional validator
        };

        RequestHandlerDelegate<Result<string>> next = () => Task.FromResult(Result<string>.Success("Success"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(invalidCommand, next, CancellationToken.None));

        exception.Errors.Should().Contain(e => e.ErrorMessage == "Age must be exactly 30 for additional validation");
    }

    [Fact]
    public async Task FluentValidationBehavior_WithAsyncValidation_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrFluentValidation();
        services.AddScoped<IValidator<TestValidationCommand>, AsyncTestValidator>();

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<TestValidationCommand, Result<string>>>();

        var command = new TestValidationCommand
        {
            Name = "John Doe",
            Email = "existing@example.com", // Will fail async validation
            Age = 25
        };

        RequestHandlerDelegate<Result<string>> next = () => Task.FromResult(Result<string>.Success("Success"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(command, next, CancellationToken.None));

        exception.Errors.Should().Contain(e => e.ErrorMessage == "Email already exists");
    }

    [Fact]
    public void ValidationException_ShouldContainDetailedErrors()
    {
        // Arrange
        var failures = new List<FluentValidation.Results.ValidationFailure>
        {
            new("Name", "Name is required"),
            new("Email", "Email must be valid"),
            new("Age", "Age must be at least 18")
        };

        // Act
        var exception = new ValidationException(failures);

        // Assert
        exception.Errors.Should().HaveCount(3);
        exception.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage == "Name is required");
        exception.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage == "Email must be valid");
        exception.Errors.Should().Contain(e => e.PropertyName == "Age" && e.ErrorMessage == "Age must be at least 18");
    }

    [Fact]
    public async Task FluentValidationBehavior_WithCancellation_ShouldRespectCancellationToken()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrFluentValidation();
        services.AddScoped<IValidator<TestValidationCommand>, SlowAsyncValidator>();

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<TestValidationCommand, Result<string>>>();

        var command = new TestValidationCommand
        {
            Name = "John Doe",
            Email = "john@example.com",
            Age = 25
        };

        RequestHandlerDelegate<Result<string>> next = () => Task.FromResult(Result<string>.Success("Success"));

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => behavior.Handle(command, next, cts.Token));
    }

    #region Additional Test Validators

    public class AdditionalTestValidator : AbstractValidator<TestValidationCommand>
    {
        public AdditionalTestValidator()
        {
            RuleFor(x => x.Age)
                .Equal(30)
                .WithMessage("Age must be exactly 30 for additional validation");
        }
    }

    public class AsyncTestValidator : AbstractValidator<TestValidationCommand>
    {
        public AsyncTestValidator()
        {
            RuleFor(x => x.Email)
                .MustAsync(async (email, cancellation) =>
                {
                    await Task.Delay(10, cancellation);
                    return email != "existing@example.com";
                })
                .WithMessage("Email already exists");
        }
    }

    public class SlowAsyncValidator : AbstractValidator<TestValidationCommand>
    {
        public SlowAsyncValidator()
        {
            RuleFor(x => x.Email)
                .MustAsync(async (email, cancellation) =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellation);
                    return true;
                })
                .WithMessage("Slow validation");
        }
    }

    #endregion
}

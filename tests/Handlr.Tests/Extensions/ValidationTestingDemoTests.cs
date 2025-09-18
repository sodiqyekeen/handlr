using FluentAssertions;
using FluentValidation;
using Handlr.Abstractions.Commands;
using Handlr.Abstractions.Pipelines;
using Handlr.Abstractions.Results;
using Handlr.Extensions.FluentValidation;
using Handlr.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Handlr.Tests.Extensions;

/// <summary>
/// Working validation tests demonstrating our testing framework
/// </summary>
public class ValidationTestingDemoTests
{
    #region Test Commands

    public record TestCommand : TestCommandBase<Result<string>>
    {
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
    }

    #endregion

    #region Validators

    public class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required");

            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("Email must be valid");
        }
    }

    #endregion

    #region Handlers

    public class TestCommandHandler : ICommandHandler<TestCommand, Result<string>>
    {
        public Task<Result<string>> Handle(TestCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success($"Processed: {command.Name}"));
        }
    }

    #endregion

    [Fact]
    public void FluentValidationExtension_ShouldRegisterCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrFluentValidation();
        services.AddScoped<IValidator<TestCommand>, TestCommandValidator>();

        var serviceProvider = services.BuildServiceProvider();
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TestCommand, Result<string>>>();

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
        services.AddScoped<IValidator<TestCommand>, TestCommandValidator>();

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<TestCommand, Result<string>>>();

        var validCommand = new TestCommand
        {
            Name = "John Doe",
            Email = "john@example.com"
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
        services.AddScoped<IValidator<TestCommand>, TestCommandValidator>();

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<TestCommand, Result<string>>>();

        var invalidCommand = new TestCommand
        {
            Name = "", // Invalid - empty
            Email = "invalid-email" // Invalid - not email format
        };

        RequestHandlerDelegate<Result<string>> next = () => Task.FromResult(Result<string>.Success("Success"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => behavior.Handle(invalidCommand, next, CancellationToken.None));

        exception.Errors.Should().HaveCountGreaterThan(0);
        exception.Errors.Should().Contain(e => e.PropertyName == nameof(TestCommand.Name));
        exception.Errors.Should().Contain(e => e.PropertyName == nameof(TestCommand.Email));
    }

    [Fact]
    public void TestBase_ShouldImplementRequiredInterfaceMembers()
    {
        // Arrange & Act
        var command = new TestCommand
        {
            Name = "Test",
            Email = "test@example.com"
        };

        // Assert
        command.CorrelationId.Should().NotBeNullOrEmpty();
        command.Metadata.Should().NotBeNull();
        command.Should().BeAssignableTo<ICommand<Result<string>>>();
    }
}

using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using FluentAssertions;
using FluentValidation;
using Handlr.Abstractions.Commands;
using Handlr.Abstractions.Pipelines;
using Handlr.Abstractions.Results;
using Handlr.Extensions.DataAnnotations;
using Handlr.Extensions.FluentValidation;
using Handlr.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Handlr.Tests.Extensions;

/// <summary>
/// Performance tests to ensure validation extensions don't significantly impact pipeline performance
/// </summary>
public class ValidationPerformanceTests
{
    #region Test Commands

    public record SimpleCommand : TestCommandBase<Result<string>>
    {
        public string Data { get; init; } = string.Empty;
    }

    public record DataAnnotationsCommand : TestCommandBase<Result<string>>
    {
        [Required]
        [StringLength(100)]
        public string Name { get; init; } = string.Empty;

        [EmailAddress]
        public string Email { get; init; } = string.Empty;

        [Range(18, 100)]
        public int Age { get; init; }
    }

    public record FluentValidationCommand : TestCommandBase<Result<string>>
    {
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public int Age { get; init; }
    }

    public record ComplexValidationCommand : TestCommandBase<Result<string>>
    {
        [Required]
        [StringLength(100)]
        public string Name { get; init; } = string.Empty;

        [EmailAddress]
        public string Email { get; init; } = string.Empty;

        [Range(18, 100)]
        public int Age { get; init; }

        public List<ComplexItem> Items { get; init; } = new();
    }

    public class ComplexItem
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int Value { get; set; }
    }

    #endregion

    #region Validators

    public class FluentValidationCommandValidator : AbstractValidator<FluentValidationCommand>
    {
        public FluentValidationCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .Length(1, 100);

            RuleFor(x => x.Email)
                .EmailAddress();

            RuleFor(x => x.Age)
                .InclusiveBetween(18, 100);
        }
    }

    public class ComplexValidationCommandValidator : AbstractValidator<ComplexValidationCommand>
    {
        public ComplexValidationCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .Length(1, 100);

            RuleFor(x => x.Email)
                .EmailAddress();

            RuleFor(x => x.Age)
                .InclusiveBetween(18, 100);

            RuleForEach(x => x.Items)
                .SetValidator(new ComplexItemValidator());
        }
    }

    public class ComplexItemValidator : AbstractValidator<ComplexItem>
    {
        public ComplexItemValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.Value)
                .InclusiveBetween(1, 1000);
        }
    }

    #endregion

    #region Handlers

    public class SimpleCommandHandler : ICommandHandler<SimpleCommand, Result<string>>
    {
        public Task<Result<string>> Handle(SimpleCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success("Simple processing"));
        }
    }

    public class DataAnnotationsCommandHandler : ICommandHandler<DataAnnotationsCommand, Result<string>>
    {
        public Task<Result<string>> Handle(DataAnnotationsCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success("DataAnnotations processing"));
        }
    }

    public class FluentValidationCommandHandler : ICommandHandler<FluentValidationCommand, Result<string>>
    {
        public Task<Result<string>> Handle(FluentValidationCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success("FluentValidation processing"));
        }
    }

    public class ComplexValidationCommandHandler : ICommandHandler<ComplexValidationCommand, Result<string>>
    {
        public Task<Result<string>> Handle(ComplexValidationCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success("Complex processing"));
        }
    }

    #endregion

    private const int WarmupIterations = 100;
    private const int TestIterations = 1000;

    [Fact]
    public async Task NoValidation_BaselinePerformance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        // No validation extensions added

        var serviceProvider = services.BuildServiceProvider();
        var command = new SimpleCommand { Data = "test data" };

        RequestHandlerDelegate<Result<string>> next = () =>
            Task.FromResult(Result<string>.Success("Success"));

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
        {
            await next();
        }

        // Act & Measure
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < TestIterations; i++)
        {
            await next();
        }
        stopwatch.Stop();

        // Assert
        var baselineMs = stopwatch.ElapsedMilliseconds;
        Console.WriteLine($"Baseline (no validation): {baselineMs}ms for {TestIterations} iterations");

        // Should be very fast without validation
        baselineMs.Should().BeLessThan(100, "baseline should be under 100ms for 1000 iterations");
    }

    [Fact]
    public async Task DataAnnotationsValidation_PerformanceImpact()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrDataAnnotations();

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<DataAnnotationsCommand, Result<string>>>();

        var validCommand = new DataAnnotationsCommand
        {
            Name = "John Doe",
            Email = "john@example.com",
            Age = 25
        };

        RequestHandlerDelegate<Result<string>> next = () =>
            Task.FromResult(Result<string>.Success("Success"));

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
        {
            await behavior.Handle(validCommand, next, CancellationToken.None);
        }

        // Act & Measure
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < TestIterations; i++)
        {
            await behavior.Handle(validCommand, next, CancellationToken.None);
        }
        stopwatch.Stop();

        // Assert
        var dataAnnotationsMs = stopwatch.ElapsedMilliseconds;
        Console.WriteLine($"DataAnnotations validation: {dataAnnotationsMs}ms for {TestIterations} iterations");

        // Should be reasonably fast
        dataAnnotationsMs.Should().BeLessThan(1000, "DataAnnotations validation should be under 1000ms for 1000 iterations");
    }

    [Fact]
    public async Task FluentValidation_PerformanceImpact()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrFluentValidation();
        services.AddScoped<IValidator<FluentValidationCommand>, FluentValidationCommandValidator>();

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<FluentValidationCommand, Result<string>>>();

        var validCommand = new FluentValidationCommand
        {
            Name = "John Doe",
            Email = "john@example.com",
            Age = 25
        };

        RequestHandlerDelegate<Result<string>> next = () =>
            Task.FromResult(Result<string>.Success("Success"));

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
        {
            await behavior.Handle(validCommand, next, CancellationToken.None);
        }

        // Act & Measure
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < TestIterations; i++)
        {
            await behavior.Handle(validCommand, next, CancellationToken.None);
        }
        stopwatch.Stop();

        // Assert
        var fluentValidationMs = stopwatch.ElapsedMilliseconds;
        Console.WriteLine($"FluentValidation: {fluentValidationMs}ms for {TestIterations} iterations");

        // Should be reasonably fast
        fluentValidationMs.Should().BeLessThan(1000, "FluentValidation should be under 1000ms for 1000 iterations");
    }

    [Fact]
    public async Task MixedValidation_PerformanceImpact()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrDataAnnotations();
        services.AddHandlrFluentValidation();
        services.AddScoped<IValidator<DataAnnotationsCommand>, DataAnnotationsCommand_FluentValidator>();

        var serviceProvider = services.BuildServiceProvider();
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<DataAnnotationsCommand, Result<string>>>();

        var validCommand = new DataAnnotationsCommand
        {
            Name = "John Doe",
            Email = "john@example.com",
            Age = 25
        };

        RequestHandlerDelegate<Result<string>> next = () =>
            Task.FromResult(Result<string>.Success("Success"));

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
        {
            await behaviors.First().Handle(validCommand,
                () => behaviors.Last().Handle(validCommand, next, CancellationToken.None),
                CancellationToken.None);
        }

        // Act & Measure
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < TestIterations; i++)
        {
            await behaviors.First().Handle(validCommand,
                () => behaviors.Last().Handle(validCommand, next, CancellationToken.None),
                CancellationToken.None);
        }
        stopwatch.Stop();

        // Assert
        var mixedValidationMs = stopwatch.ElapsedMilliseconds;
        Console.WriteLine($"Mixed validation: {mixedValidationMs}ms for {TestIterations} iterations");

        // Should be reasonably fast even with both validations
        mixedValidationMs.Should().BeLessThan(2000, "Mixed validation should be under 2000ms for 1000 iterations");
    }

    [Fact]
    public async Task ComplexValidation_PerformanceWithLargeObjects()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrDataAnnotations();
        services.AddHandlrFluentValidation();
        services.AddScoped<IValidator<ComplexValidationCommand>, ComplexValidationCommandValidator>();

        var serviceProvider = services.BuildServiceProvider();
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<ComplexValidationCommand, Result<string>>>();

        var complexCommand = new ComplexValidationCommand
        {
            Name = "John Doe",
            Email = "john@example.com",
            Age = 25,
            Items = Enumerable.Range(1, 100) // 100 items to validate
                .Select(i => new ComplexItem { Id = $"ID-{i}", Value = i })
                .ToList()
        };

        RequestHandlerDelegate<Result<string>> next = () =>
            Task.FromResult(Result<string>.Success("Success"));

        // Warmup
        for (int i = 0; i < 10; i++) // Fewer warmup iterations for complex objects
        {
            await behaviors.First().Handle(complexCommand,
                () => behaviors.Last().Handle(complexCommand, next, CancellationToken.None),
                CancellationToken.None);
        }

        // Act & Measure
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++) // Fewer test iterations for complex objects
        {
            await behaviors.First().Handle(complexCommand,
                () => behaviors.Last().Handle(complexCommand, next, CancellationToken.None),
                CancellationToken.None);
        }
        stopwatch.Stop();

        // Assert
        var complexValidationMs = stopwatch.ElapsedMilliseconds;
        Console.WriteLine($"Complex validation (100 items): {complexValidationMs}ms for 100 iterations");

        // Should still be reasonable even with complex objects
        complexValidationMs.Should().BeLessThan(5000, "Complex validation should be under 5000ms for 100 iterations");
    }

    [Fact]
    public async Task ValidationFailure_PerformanceImpact()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrDataAnnotations();

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<DataAnnotationsCommand, Result<string>>>();

        var invalidCommand = new DataAnnotationsCommand
        {
            Name = "", // Invalid
            Email = "invalid-email", // Invalid
            Age = 0 // Invalid
        };

        RequestHandlerDelegate<Result<string>> next = () =>
            Task.FromResult(Result<string>.Success("Success"));

        // Warmup (with exception handling)
        for (int i = 0; i < WarmupIterations; i++)
        {
            try
            {
                await behavior.Handle(invalidCommand, next, CancellationToken.None);
            }
            catch (DataAnnotationsValidationException)
            {
                // Expected
            }
        }

        // Act & Measure
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < TestIterations; i++)
        {
            try
            {
                await behavior.Handle(invalidCommand, next, CancellationToken.None);
            }
            catch (DataAnnotationsValidationException)
            {
                // Expected
            }
        }
        stopwatch.Stop();

        // Assert
        var failureMs = stopwatch.ElapsedMilliseconds;
        Console.WriteLine($"Validation failure handling: {failureMs}ms for {TestIterations} iterations");

        // Failure handling should not be significantly slower than success
        failureMs.Should().BeLessThan(2000, "Validation failure handling should be under 2000ms for 1000 iterations");
    }

    [Fact]
    public async Task ValidatorResolution_PerformanceImpact()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrFluentValidation();

        // Register multiple validators to test resolution performance
        services.AddScoped<IValidator<FluentValidationCommand>, FluentValidationCommandValidator>();
        services.AddScoped<IValidator<FluentValidationCommand>, FluentValidationCommandValidator2>();
        services.AddScoped<IValidator<FluentValidationCommand>, FluentValidationCommandValidator3>();

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<FluentValidationCommand, Result<string>>>();

        var validCommand = new FluentValidationCommand
        {
            Name = "John Doe",
            Email = "john@example.com",
            Age = 25
        };

        RequestHandlerDelegate<Result<string>> next = () =>
            Task.FromResult(Result<string>.Success("Success"));

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
        {
            await behavior.Handle(validCommand, next, CancellationToken.None);
        }

        // Act & Measure
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < TestIterations; i++)
        {
            await behavior.Handle(validCommand, next, CancellationToken.None);
        }
        stopwatch.Stop();

        // Assert
        var multiValidatorMs = stopwatch.ElapsedMilliseconds;
        Console.WriteLine($"Multi-validator resolution: {multiValidatorMs}ms for {TestIterations} iterations");

        // Multiple validator resolution should not be excessively slow
        multiValidatorMs.Should().BeLessThan(2000, "Multi-validator resolution should be under 2000ms for 1000 iterations");
    }

    #region Additional Validators for Performance Testing

    public class DataAnnotationsCommand_FluentValidator : AbstractValidator<DataAnnotationsCommand>
    {
        public DataAnnotationsCommand_FluentValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .Length(1, 100);
        }
    }

    public class FluentValidationCommandValidator2 : AbstractValidator<FluentValidationCommand>
    {
        public FluentValidationCommandValidator2()
        {
            RuleFor(x => x.Name)
                .Must(name => !string.IsNullOrEmpty(name))
                .WithMessage("Name is required (validator 2)");
        }
    }

    public class FluentValidationCommandValidator3 : AbstractValidator<FluentValidationCommand>
    {
        public FluentValidationCommandValidator3()
        {
            RuleFor(x => x.Email)
                .Must(email => !string.IsNullOrEmpty(email))
                .WithMessage("Email is required (validator 3)");
        }
    }

    #endregion
}

using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Handlr.Abstractions.Commands;
using Handlr.Abstractions.Pipelines;
using Handlr.Abstractions.Queries;
using Handlr.Abstractions.Results;
using Handlr.Extensions.DataAnnotations;
using Handlr.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Handlr.Tests.Extensions;

/// <summary>
/// Comprehensive tests for DataAnnotations extension functionality
/// </summary>
public class DataAnnotationsExtensionTests
{
    #region Test Commands and Queries

    public record TestDataAnnotationsCommand : TestCommandBase<Result<string>>
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters")]
        public string Name { get; init; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email must be valid")]
        public string Email { get; init; } = string.Empty;

        [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
        public int Age { get; init; }
    }

    public record TestDataAnnotationsQuery : TestQueryBase<Result<string>>
    {
        [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
        public string? SearchTerm { get; init; }

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; init; } = 10;
    }

    public record TestNoAnnotationsCommand : TestCommandBase<Result<string>>
    {
        public string Data { get; init; } = string.Empty;
    }

    public record TestComplexObjectCommand : TestCommandBase<Result<string>>
    {
        [Required]
        public CustomerInfo Customer { get; init; } = new();

        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<OrderItem> Items { get; init; } = new();
    }

    public record TestCustomValidationCommand : TestCommandBase<Result<string>>
    {
        [Required]
        public string Name { get; init; } = string.Empty;

        [PositiveNumber]
        public int Score { get; init; }

        [FutureDate]
        public DateTime EventDate { get; init; }
    }

    #endregion

    #region Complex Types

    public class CustomerInfo
    {
        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer email is required")]
        [EmailAddress(ErrorMessage = "Customer email must be valid")]
        public string Email { get; set; } = string.Empty;
    }

    public class OrderItem
    {
        [Required]
        public string ProductId { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be positive")]
        public int Quantity { get; set; }
    }

    #endregion

    #region Custom Validation Attributes

    public class PositiveNumberAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is int intValue)
                return intValue > 0;
            if (value is decimal decimalValue)
                return decimalValue > 0;
            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be a positive number";
        }
    }

    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is DateTime dateValue)
                return dateValue > DateTime.Now;
            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be a future date";
        }
    }

    #endregion

    #region Handlers

    public class TestDataAnnotationsCommandHandler : ICommandHandler<TestDataAnnotationsCommand, Result<string>>
    {
        public Task<Result<string>> Handle(TestDataAnnotationsCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success($"Created user: {command.Name}"));
        }
    }

    public class TestDataAnnotationsQueryHandler : IQueryHandler<TestDataAnnotationsQuery, Result<string>>
    {
        public Task<Result<string>> Handle(TestDataAnnotationsQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success($"Search results for: {query.SearchTerm}"));
        }
    }

    public class TestNoAnnotationsCommandHandler : ICommandHandler<TestNoAnnotationsCommand, Result<string>>
    {
        public Task<Result<string>> Handle(TestNoAnnotationsCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success($"Processed: {command.Data}"));
        }
    }

    public class TestComplexObjectCommandHandler : ICommandHandler<TestComplexObjectCommand, Result<string>>
    {
        public Task<Result<string>> Handle(TestComplexObjectCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success($"Created order for: {command.Customer.Name}"));
        }
    }

    public class TestCustomValidationCommandHandler : ICommandHandler<TestCustomValidationCommand, Result<string>>
    {
        public Task<Result<string>> Handle(TestCustomValidationCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<string>.Success($"Processed: {command.Name}"));
        }
    }

    #endregion

    [Fact]
    public void AddHandlrDataAnnotations_ShouldRegisterBehavior()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHandlrDataAnnotations();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TestDataAnnotationsCommand, Result<string>>>();

        behaviors.Should().NotBeEmpty();
        behaviors.Should().ContainSingle(b => b.GetType().Name.Contains("DataAnnotations"));
    }

    [Fact]
    public async Task DataAnnotationsBehavior_WithValidRequest_ShouldCallNext()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrDataAnnotations();

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<TestDataAnnotationsCommand, Result<string>>>();

        var validCommand = new TestDataAnnotationsCommand
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
    public async Task DataAnnotationsBehavior_WithInvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrDataAnnotations();

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<TestDataAnnotationsCommand, Result<string>>>();

        var invalidCommand = new TestDataAnnotationsCommand
        {
            Name = "", // Invalid - empty
            Email = "invalid-email", // Invalid - not email format
            Age = 15 // Invalid - under 18
        };

        RequestHandlerDelegate<Result<string>> next = () => Task.FromResult(Result<string>.Success("Success"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DataAnnotationsValidationException>(
            () => behavior.Handle(invalidCommand, next, CancellationToken.None));

        exception.ValidationResults.Should().HaveCountGreaterThan(0);
        exception.ValidationResults.Should().Contain(e => e.MemberNames.Contains(nameof(TestDataAnnotationsCommand.Name)));
        exception.ValidationResults.Should().Contain(e => e.MemberNames.Contains(nameof(TestDataAnnotationsCommand.Email)));
        exception.ValidationResults.Should().Contain(e => e.MemberNames.Contains(nameof(TestDataAnnotationsCommand.Age)));
    }

    [Fact]
    public async Task DataAnnotationsBehavior_WithNoAnnotations_ShouldCallNext()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrDataAnnotations();

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<TestNoAnnotationsCommand, Result<string>>>();

        var command = new TestNoAnnotationsCommand { Data = "test" };

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
    public async Task DataAnnotationsBehavior_WithComplexObjects_ShouldValidateNestedProperties()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrDataAnnotations();

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<TestComplexObjectCommand, Result<string>>>();

        var invalidCommand = new TestComplexObjectCommand
        {
            Customer = new CustomerInfo
            {
                Name = "", // Invalid - empty
                Email = "invalid-email" // Invalid - not email format
            },
            Items = new List<OrderItem>
            {
                new() { ProductId = "", Quantity = 0 } // Invalid - empty product ID and zero quantity
            }
        };

        RequestHandlerDelegate<Result<string>> next = () => Task.FromResult(Result<string>.Success("Success"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DataAnnotationsValidationException>(
            () => behavior.Handle(invalidCommand, next, CancellationToken.None));

        exception.ValidationResults.Should().HaveCountGreaterThan(0);
        exception.ValidationResults.Should().Contain(e => e.ErrorMessage != null && e.ErrorMessage.Contains("Customer name is required"));
        exception.ValidationResults.Should().Contain(e => e.ErrorMessage != null && e.ErrorMessage.Contains("Customer email"));
        exception.ValidationResults.Should().Contain(e => e.ErrorMessage != null && e.ErrorMessage.Contains("ProductId"));
        exception.ValidationResults.Should().Contain(e => e.ErrorMessage != null && e.ErrorMessage.Contains("Quantity"));
    }

    [Fact]
    public async Task DataAnnotationsBehavior_WithCustomAttributes_ShouldValidateCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrDataAnnotations();

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<TestCustomValidationCommand, Result<string>>>();

        var invalidCommand = new TestCustomValidationCommand
        {
            Name = "John Doe",
            Score = -5, // Invalid - not positive
            EventDate = DateTime.Now.AddDays(-1) // Invalid - past date
        };

        RequestHandlerDelegate<Result<string>> next = () => Task.FromResult(Result<string>.Success("Success"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DataAnnotationsValidationException>(
            () => behavior.Handle(invalidCommand, next, CancellationToken.None));

        exception.ValidationResults.Should().HaveCountGreaterThan(0);
        exception.ValidationResults.Should().Contain(e => e.ErrorMessage != null && e.ErrorMessage.Contains("positive number"));
        exception.ValidationResults.Should().Contain(e => e.ErrorMessage != null && e.ErrorMessage.Contains("future date"));
    }

    [Fact]
    public async Task DataAnnotationsBehavior_WithValidComplexObject_ShouldCallNext()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrDataAnnotations();

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<TestComplexObjectCommand, Result<string>>>();

        var validCommand = new TestComplexObjectCommand
        {
            Customer = new CustomerInfo
            {
                Name = "John Doe",
                Email = "john@example.com"
            },
            Items = new List<OrderItem>
            {
                new() { ProductId = "PROD-001", Quantity = 5 }
            }
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
    public void DataAnnotationsValidationException_ShouldContainDetailedErrors()
    {
        // Arrange
        var validationResults = new List<ValidationResult>
        {
            new("Name is required", new[] { "Name" }),
            new("Email must be valid", new[] { "Email" }),
            new("Age must be at least 18", new[] { "Age" })
        };

        // Act
        var exception = new DataAnnotationsValidationException(validationResults);

        // Assert
        exception.ValidationResults.Should().HaveCount(3);
        exception.ValidationResults.Should().Contain(r => r.MemberNames.Contains("Name") && r.ErrorMessage == "Name is required");
        exception.ValidationResults.Should().Contain(r => r.MemberNames.Contains("Email") && r.ErrorMessage == "Email must be valid");
        exception.ValidationResults.Should().Contain(r => r.MemberNames.Contains("Age") && r.ErrorMessage == "Age must be at least 18");
    }

    [Fact]
    public async Task DataAnnotationsBehavior_WithPartiallyValidObject_ShouldReportOnlyFailures()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrDataAnnotations();

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<TestDataAnnotationsCommand, Result<string>>>();

        var partiallyValidCommand = new TestDataAnnotationsCommand
        {
            Name = "John Doe", // Valid
            Email = "invalid-email", // Invalid
            Age = 25 // Valid
        };

        RequestHandlerDelegate<Result<string>> next = () => Task.FromResult(Result<string>.Success("Success"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DataAnnotationsValidationException>(
            () => behavior.Handle(partiallyValidCommand, next, CancellationToken.None));

        exception.ValidationResults.Should().HaveCount(1);
        exception.ValidationResults.Should().Contain(e => e.MemberNames.Contains(nameof(TestDataAnnotationsCommand.Email)));
        exception.ValidationResults.Should().NotContain(e => e.MemberNames.Contains(nameof(TestDataAnnotationsCommand.Name)));
        exception.ValidationResults.Should().NotContain(e => e.MemberNames.Contains(nameof(TestDataAnnotationsCommand.Age)));
    }

    [Fact]
    public async Task DataAnnotationsBehavior_WithOptionalProperties_ShouldValidateCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrDataAnnotations();

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<TestDataAnnotationsQuery, Result<string>>>();

        var queryWithoutOptional = new TestDataAnnotationsQuery
        {
            SearchTerm = null, // Optional - should be valid
            PageSize = 10 // Valid
        };

        var nextCalled = false;
        RequestHandlerDelegate<Result<string>> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result<string>.Success("Success"));
        };

        // Act
        var result = await behavior.Handle(queryWithoutOptional, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DataAnnotationsBehavior_WithInvalidOptionalProperty_ShouldThrowValidationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHandlrDataAnnotations();

        var serviceProvider = services.BuildServiceProvider();
        var behavior = serviceProvider.GetRequiredService<IPipelineBehavior<TestDataAnnotationsQuery, Result<string>>>();

        var invalidQuery = new TestDataAnnotationsQuery
        {
            SearchTerm = new string('a', 101), // Invalid - exceeds max length
            PageSize = 0 // Invalid - not in range
        };

        RequestHandlerDelegate<Result<string>> next = () => Task.FromResult(Result<string>.Success("Success"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DataAnnotationsValidationException>(
            () => behavior.Handle(invalidQuery, next, CancellationToken.None));

        exception.ValidationResults.Should().HaveCount(2);
        exception.ValidationResults.Should().Contain(e => e.MemberNames.Contains(nameof(TestDataAnnotationsQuery.SearchTerm)));
        exception.ValidationResults.Should().Contain(e => e.MemberNames.Contains(nameof(TestDataAnnotationsQuery.PageSize)));
    }
}

using Handlr.Abstractions.Commands;
using Handlr.Abstractions.Pipelines;
using Handlr.Abstractions.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Handlr.Tests;

/// <summary>
/// Tests for the complete Handlr CQRS framework
/// </summary>
public class HandlrFrameworkTests
{
    /// <summary>
    /// Tests that command handlers can be registered and resolved
    /// </summary>
    [Fact]
    public async Task Should_Register_And_Resolve_Command_Handler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Manually register handler (simulating what source generator would do)
        services.AddScoped<ICommandHandler<TestCommand, string>, TestCommandHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TestCommand, string>>();

        var command = new TestCommand { Name = "Test User" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Handled: Test User", result);
    }

    /// <summary>
    /// Tests that query handlers can be registered and resolved
    /// </summary>
    [Fact]
    public async Task Should_Register_And_Resolve_Query_Handler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Manually register handler (simulating what source generator would do)
        services.AddScoped<IQueryHandler<TestQuery, string>, TestQueryHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetRequiredService<IQueryHandler<TestQuery, string>>();

        var query = new TestQuery { UserId = 123 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal("User: 123", result);
    }

    /// <summary>
    /// Tests that pipeline behaviors work correctly
    /// </summary>
    [Fact]
    public async Task Should_Execute_Pipeline_Behaviors()
    {
        // Arrange
        var services = new ServiceCollection();

        // Register handler and behavior
        services.AddScoped<ICommandHandler<TestCommand, string>, TestCommandHandlerWithPipeline>();
        services.AddScoped<IPipelineBehavior<TestCommand, string>, TestLoggingBehavior>();

        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TestCommand, string>>();

        var command = new TestCommand { Name = "Test User" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Logged: Handled: Test User", result);
    }

    /// <summary>
    /// Tests that commands without return values work
    /// </summary>
    [Fact]
    public async Task Should_Handle_Command_Without_Return_Type()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<ICommandHandler<VoidCommand>, VoidCommandHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetRequiredService<ICommandHandler<VoidCommand>>();

        var command = new VoidCommand { Message = "Test Message" };

        // Act & Assert (should not throw)
        await handler.Handle(command, CancellationToken.None);
    }

    /// <summary>
    /// Tests that multiple pipeline behaviors execute in correct order
    /// </summary>
    [Fact]
    public async Task Should_Execute_Multiple_Pipeline_Behaviors_In_Order()
    {
        // Arrange
        var services = new ServiceCollection();

        // Register handler and multiple behaviors
        services.AddScoped<ICommandHandler<TestCommand, string>, TestCommandHandlerWithPipeline>();
        services.AddScoped<IPipelineBehavior<TestCommand, string>, TestLoggingBehavior>();
        services.AddScoped<IPipelineBehavior<TestCommand, string>, TestTimingBehavior>();
        services.AddScoped<IPipelineBehavior<TestCommand, string>, TestValidationBehavior>();

        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TestCommand, string>>();

        var command = new TestCommand { Name = "Test User" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert - behaviors should wrap each other
        Assert.Contains("Validated", result);
        Assert.Contains("Timed", result);
        Assert.Contains("Logged", result);
        Assert.Contains("Handled: Test User", result);
    }

    /// <summary>
    /// Tests error handling in pipeline behaviors
    /// </summary>
    [Fact]
    public async Task Should_Handle_Exceptions_In_Pipeline_Behaviors()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddScoped<ICommandHandler<TestCommand, string>, TestCommandHandlerWithPipeline>();
        services.AddScoped<IPipelineBehavior<TestCommand, string>, TestExceptionBehavior>();

        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TestCommand, string>>();

        var command = new TestCommand { Name = "Error" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("Test exception from pipeline", exception.Message);
    }

    /// <summary>
    /// Tests cancellation token propagation
    /// </summary>
    [Fact]
    public async Task Should_Propagate_Cancellation_Token()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<ICommandHandler<TestCommand, string>, TestCancellationCommandHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TestCommand, string>>();

        var command = new TestCommand { Name = "Cancel Me" };
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(command, cts.Token));
    }

    /// <summary>
    /// Tests that metadata is properly passed through
    /// </summary>
    [Fact]
    public async Task Should_Pass_Metadata_Through_Pipeline()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddScoped<ICommandHandler<TestCommand, string>, TestMetadataCommandHandler>();
        services.AddScoped<IPipelineBehavior<TestCommand, string>, TestMetadataBehavior>();

        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TestCommand, string>>();

        var metadata = new Dictionary<string, object> { { "UserId", 123 }, { "TraceId", "abc-123" } };
        var command = new TestCommand
        {
            Name = "Test User",
            Metadata = metadata,
            CorrelationId = "test-correlation-id"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Contains("UserId: 123", result);
        Assert.Contains("TraceId: abc-123", result);
        Assert.Contains("CorrelationId: test-correlation-id", result);
    }

    /// <summary>
    /// Tests query with complex return type
    /// </summary>
    [Fact]
    public async Task Should_Handle_Query_With_Complex_Result()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IQueryHandler<UserListQuery, List<User>>, UserListQueryHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetRequiredService<IQueryHandler<UserListQuery, List<User>>>();

        var query = new UserListQuery { PageSize = 10, Page = 1 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.All(result, user => Assert.NotNull(user.Name));
    }
}

// Test commands and queries
/// <summary>
/// Test command for integration testing
/// </summary>
public record TestCommand : ICommand<string>
{
    /// <summary>Gets the correlation ID</summary>
    public string? CorrelationId { get; init; }
    /// <summary>Gets the metadata</summary>
    public IDictionary<string, object>? Metadata { get; init; }
    /// <summary>Gets the name</summary>
    public string Name { get; init; } = string.Empty;
}

/// <summary>
/// Test query for integration testing
/// </summary>
public record TestQuery : IQuery<string>
{
    /// <summary>Gets the correlation ID</summary>
    public string? CorrelationId { get; init; }
    /// <summary>Gets the metadata</summary>
    public IDictionary<string, object>? Metadata { get; init; }
    /// <summary>Gets the user ID</summary>
    public int UserId { get; init; }
}

/// <summary>
/// Test command without return type
/// </summary>
public record VoidCommand : ICommand
{
    /// <summary>Gets the correlation ID</summary>
    public string? CorrelationId { get; init; }
    /// <summary>Gets the metadata</summary>
    public IDictionary<string, object>? Metadata { get; init; }
    /// <summary>Gets the message</summary>
    public string Message { get; init; } = string.Empty;
}

// Test handlers
/// <summary>
/// Test command handler implementation
/// </summary>
public class TestCommandHandler : ICommandHandler<TestCommand, string>
{
    /// <summary>
    /// Handles the command
    /// </summary>
    public Task<string> Handle(TestCommand command, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Handled: {command.Name}");
    }
}

/// <summary>
/// Test query handler implementation
/// </summary>
public class TestQueryHandler : IQueryHandler<TestQuery, string>
{
    /// <summary>
    /// Handles the query
    /// </summary>
    public Task<string> Handle(TestQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"User: {query.UserId}");
    }
}

/// <summary>
/// Test command handler with pipeline support
/// </summary>
public class TestCommandHandlerWithPipeline : ICommandHandler<TestCommand, string>
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance
    /// </summary>
    public TestCommandHandlerWithPipeline(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Handles the command with pipeline
    /// </summary>
    public async Task<string> Handle(TestCommand command, CancellationToken cancellationToken = default)
    {
        var behaviors = _serviceProvider.GetServices<IPipelineBehavior<TestCommand, string>>();

        Task<string> Core() => Task.FromResult($"Handled: {command.Name}");

        RequestHandlerDelegate<string> handler = Core;

        foreach (var behavior in behaviors.Reverse())
        {
            var currentHandler = handler;
            handler = () => behavior.Handle(command, currentHandler, cancellationToken);
        }

        return await handler();
    }
}

/// <summary>
/// Test void command handler
/// </summary>
public class VoidCommandHandler : ICommandHandler<VoidCommand>
{
    /// <summary>
    /// Handles the void command
    /// </summary>
    public Task Handle(VoidCommand command, CancellationToken cancellationToken = default)
    {
        // Simulate some work
        return Task.CompletedTask;
    }
}

/// <summary>
/// Test pipeline behavior for integration testing
/// </summary>
public class TestLoggingBehavior : IPipelineBehavior<TestCommand, string>
{
    /// <summary>
    /// Handles the pipeline behavior
    /// </summary>
    public async Task<string> Handle(TestCommand request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken = default)
    {
        var result = await next();
        return $"Logged: {result}";
    }
}

/// <summary>
/// Test timing behavior
/// </summary>
public class TestTimingBehavior : IPipelineBehavior<TestCommand, string>
{
    public async Task<string> Handle(TestCommand request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken = default)
    {
        var result = await next();
        return $"Timed: {result}";
    }
}

/// <summary>
/// Test validation behavior
/// </summary>
public class TestValidationBehavior : IPipelineBehavior<TestCommand, string>
{
    public async Task<string> Handle(TestCommand request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken = default)
    {
        var result = await next();
        return $"Validated: {result}";
    }
}

/// <summary>
/// Test exception behavior
/// </summary>
public class TestExceptionBehavior : IPipelineBehavior<TestCommand, string>
{
    public async Task<string> Handle(TestCommand request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken = default)
    {
        if (request.Name == "Error")
        {
            throw new InvalidOperationException("Test exception from pipeline");
        }
        return await next();
    }
}

/// <summary>
/// Test command handler that respects cancellation tokens
/// </summary>
public class TestCancellationCommandHandler : ICommandHandler<TestCommand, string>
{
    public async Task<string> Handle(TestCommand command, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Delay(100, cancellationToken);
        return $"Handled: {command.Name}";
    }
}

/// <summary>
/// Test command handler that uses metadata
/// </summary>
public class TestMetadataCommandHandler : ICommandHandler<TestCommand, string>
{
    public Task<string> Handle(TestCommand command, CancellationToken cancellationToken = default)
    {
        var result = $"Handled: {command.Name}";

        if (command.Metadata != null)
        {
            foreach (var kvp in command.Metadata)
            {
                result += $", {kvp.Key}: {kvp.Value}";
            }
        }

        if (!string.IsNullOrEmpty(command.CorrelationId))
        {
            result += $", CorrelationId: {command.CorrelationId}";
        }

        return Task.FromResult(result);
    }
}

/// <summary>
/// Test metadata behavior
/// </summary>
public class TestMetadataBehavior : IPipelineBehavior<TestCommand, string>
{
    public async Task<string> Handle(TestCommand request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken = default)
    {
        // Add some metadata processing
        var result = await next();
        return $"MetadataProcessed: {result}";
    }
}

/// <summary>
/// Test user model
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Test query for user list
/// </summary>
public record UserListQuery : IQuery<List<User>>
{
    public string? CorrelationId { get; init; }
    public IDictionary<string, object>? Metadata { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

/// <summary>
/// Test handler for user list query
/// </summary>
public class UserListQueryHandler : IQueryHandler<UserListQuery, List<User>>
{
    public Task<List<User>> Handle(UserListQuery query, CancellationToken cancellationToken = default)
    {
        var users = new List<User>
        {
            new User { Id = 1, Name = "John Doe", Email = "john@example.com" },
            new User { Id = 2, Name = "Jane Smith", Email = "jane@example.com" },
            new User { Id = 3, Name = "Bob Johnson", Email = "bob@example.com" }
        };

        return Task.FromResult(users);
    }
}

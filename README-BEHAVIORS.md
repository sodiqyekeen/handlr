# Pipeline Behaviors Development Guide

This document provides comprehensive guidance for implementing pipeline behaviors in the Handlr CQRS framework.

## 🎯 Core Philosophy

The Handlr framework follows these principles for pipeline behaviors:

1. **Example-Driven**: We provide comprehensive examples, not framework implementations
2. **Interface-Based**: Behaviors are triggered by interfaces that requests implement
3. **Composable**: Multiple behaviors can be applied to the same request
4. **Configurable**: Developers control which behaviors apply and in what order
5. **Source Generator Friendly**: Works seamlessly with generated handler registration

## 🏗️ Architecture Overview

```
Request → [Authorization] → [Validation] → [Logging] → [Metrics] → [Caching] → [Retry] → Handler → Response
                ↑              ↑            ↑          ↑           ↑          ↑
             Optional       Optional     Always     Optional    Queries    Optional
          (if implements  (if implements  Applied  (if implements Only    (if implements
         IRequireAuth...)  IValidatable)           IMetrics...)            IRetryable)
```

## 📚 Behavior Categories

### 1. Security Behaviors
- **AuthorizationBehaviorExample**: Permission and role-based access control
- **Audit**: Track who performed what actions (future example)

### 2. Data Quality Behaviors  
- **ValidationBehaviorExample**: Request validation with detailed error reporting
- **Sanitization**: Input cleaning and normalization (future example)

### 3. Performance Behaviors
- **CachingBehaviorExample**: Query result caching with configurable expiration
- **Throttling**: Rate limiting and request throttling (future example)

### 4. Reliability Behaviors
- **RetryBehaviorExample**: Transient failure handling with exponential backoff
- **Circuit Breaker**: Fail-fast when downstream services are down (future example)

### 5. Observability Behaviors
- **LoggingBehaviorExample**: Comprehensive request/response logging
- **MetricsBehaviorExample**: Performance and usage metrics collection
- **Tracing**: Distributed tracing support (future example)

## 🔧 Implementation Patterns

### Pattern 1: Marker Interface
```csharp
public interface IValidatable
{
    ValidationResult Validate();
}

public class ValidationBehaviorExample<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IValidatable
{
    // Only applies to requests implementing IValidatable
}
```

### Pattern 2: Configuration Interface
```csharp
public interface ICacheable
{
    string GetCacheKey();
    TimeSpan GetCacheDuration();
}
```

### Pattern 3: Metadata Interface
```csharp
public interface IMetricsEnabled
{
    string OperationName { get; }
    Dictionary<string, string> MetricsTags { get; }
}
```

## 🚀 Quick Start Guide

### Step 1: Choose Your Behaviors
Start with the most valuable behaviors for your application:
- **Logging**: Almost always valuable for debugging and monitoring
- **Validation**: Essential for data quality and security
- **Authorization**: Required for any application with users/permissions

### Step 2: Copy and Adapt Examples
```bash
# Copy behavior examples to your project
cp samples/SampleConsoleApp/Examples/LoggingBehaviorExample.cs src/MyApp/Behaviors/
cp samples/SampleConsoleApp/Examples/ValidationBehaviorExample.cs src/MyApp/Behaviors/
```

### Step 3: Implement Interfaces in Your Commands/Queries
```csharp
public class CreateUserCommand : IValidatable, IRequireAuthorization
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // IValidatable implementation
    public ValidationResult Validate()
    {
        var result = new ValidationResult();
        if (string.IsNullOrWhiteSpace(Name)) result.AddError("Name is required");
        if (!IsValidEmail(Email)) result.AddError("Invalid email");
        return result;
    }
    
    // IRequireAuthorization implementation  
    public string RequiredPermission => "User.Create";
    public string? ResourceId => null; // System-level operation
}
```

### Step 4: Register Behaviors in DI Container
```csharp
// Register in order of execution preference
services.AddScoped<IPipelineBehavior<CreateUserCommand, Result<User>>, AuthorizationBehaviorExample<CreateUserCommand, Result<User>>>();
services.AddScoped<IPipelineBehavior<CreateUserCommand, Result<User>>, ValidationBehaviorExample<CreateUserCommand, Result<User>>>();
services.AddScoped<IPipelineBehavior<CreateUserCommand, Result<User>>, LoggingBehaviorExample<CreateUserCommand, Result<User>>>();
services.AddScoped<IPipelineBehavior<CreateUserCommand, Result<User>>, MetricsBehaviorExample<CreateUserCommand, Result<User>>>();
```

## 📋 Behavior Execution Order

The order you register behaviors matters. Here's a recommended order:

1. **Authorization** - Fail fast if user doesn't have permission
2. **Validation** - Catch invalid requests early  
3. **Logging** - Log all requests (including failed ones)
4. **Metrics** - Track all operations
5. **Caching** - Check cache before expensive operations (queries only)
6. **Retry** - Handle transient failures
7. **Handler** - Execute business logic

## 🎛️ Configuration Patterns

### Option 1: Global Behavior Registration
```csharp
// Apply to all commands of a specific type
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviorExample<,>));
```

### Option 2: Selective Behavior Registration  
```csharp
// Apply only to specific command/response combinations
services.AddScoped<IPipelineBehavior<CreateUserCommand, Result<User>>, ValidationBehaviorExample<CreateUserCommand, Result<User>>>();
```

### Option 3: Conditional Registration
```csharp
// Register based on environment or configuration
if (environment.IsProduction())
{
    services.AddScoped<IPipelineBehavior<CreateUserCommand, Result<User>>, MetricsBehaviorExample<CreateUserCommand, Result<User>>>();
}
```

## 🧪 Testing Strategies

### Unit Testing Individual Behaviors
```csharp
[Test]
public async Task ValidationBehavior_ShouldRejectInvalidRequest()
{
    // Arrange
    var behavior = new ValidationBehaviorExample<InvalidCommand, Result>();
    var invalidCommand = new InvalidCommand { Name = "" }; // Invalid
    var mockNext = new Mock<RequestHandlerDelegate<Result>>();
    
    // Act & Assert
    var exception = await Assert.ThrowsAsync<ValidationException>(
        () => behavior.Handle(invalidCommand, mockNext.Object, CancellationToken.None)
    );
    
    mockNext.Verify(x => x(), Times.Never); // Handler should not be called
}
```

### Integration Testing Behavior Pipeline
```csharp
[Test]
public async Task Command_ShouldExecuteBehaviorsInCorrectOrder()
{
    // Arrange - set up full DI container with behaviors
    var services = new ServiceCollection();
    services.AddScoped<IPipelineBehavior<TestCommand, Result>, LoggingBehaviorExample<TestCommand, Result>>();
    services.AddScoped<IPipelineBehavior<TestCommand, Result>, ValidationBehaviorExample<TestCommand, Result>>();
    // ... register handler
    
    var provider = services.BuildServiceProvider();
    var mediator = provider.GetRequiredService<IMediator>();
    
    // Act
    var result = await mediator.Send(new TestCommand());
    
    // Assert - verify expected behavior execution
}
```

## 🔍 Debugging and Troubleshooting

### Common Issues

1. **Behaviors Not Executing**
   - Check DI registration
   - Verify request implements required interface
   - Check generic type constraints match

2. **Wrong Execution Order**  
   - Behaviors execute in registration order
   - Re-order registrations to fix

3. **Performance Issues**
   - Too many behaviors can add latency
   - Profile and optimize critical paths
   - Consider async/await best practices

### Debugging Tips
```csharp
// Add debug logging to behaviors
Console.WriteLine($"[DEBUG] {GetType().Name} executing for {typeof(TRequest).Name}");

// Use stopwatch for performance debugging
var stopwatch = Stopwatch.StartNew();
var result = await next();
stopwatch.Stop();
Console.WriteLine($"[PERF] {GetType().Name} took {stopwatch.ElapsedMilliseconds}ms");
```

## 🚀 Advanced Patterns

### Conditional Behavior Execution
```csharp
public class ConditionalValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IValidatable
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Only validate in development/staging
        if (Environment.IsDevelopment() || Environment.IsStaging())
        {
            var validation = request.Validate();
            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);
        }
        
        return await next();
    }
}
```

### Behavior Composition
```csharp
// Combine multiple concerns in one behavior
public class SecurityAndAuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequireAuthorization
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // 1. Check authorization
        await CheckAuthorization(request);
        
        // 2. Audit the attempt
        await AuditRequest(request);
        
        // 3. Execute and audit result
        var result = await next();
        await AuditResult(request, result);
        
        return result;
    }
}
```

### Dynamic Behavior Selection
```csharp
// Choose behaviors based on request properties
public class DynamicRetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var retryConfig = GetRetryConfigForRequest(request);
        
        if (retryConfig.MaxRetries == 0)
            return await next(); // No retry
            
        return await ExecuteWithRetry(next, retryConfig, cancellationToken);
    }
}
```

## 🎯 Best Practices Summary

1. **Start Simple**: Begin with logging and validation behaviors
2. **Test Thoroughly**: Unit test behaviors independently
3. **Monitor Performance**: Behaviors add overhead - measure it
4. **Be Selective**: Not every request needs every behavior
5. **Document Interfaces**: Make behavior contracts clear
6. **Handle Exceptions**: Behaviors should fail gracefully
7. **Use Configuration**: Make behaviors configurable via options
8. **Consider Order**: Registration order determines execution order
9. **Keep Focused**: One behavior, one concern
10. **Provide Examples**: Help team members understand patterns

## 📖 Further Reading

- [samples/SampleConsoleApp/Examples/](./samples/SampleConsoleApp/Examples/) - Complete behavior examples
- [samples/SampleConsoleApp/Examples/README.md](./samples/SampleConsoleApp/Examples/README.md) - Detailed example documentation
- Source generator documentation (when available)
- CQRS pattern documentation
# Pipeline Behavior Examples

This directory contains comprehensive examples of how to implement pipeline behaviors in the Handlr CQRS framework. These behaviors provide cross-cutting concerns that can be applied to commands and queries.

## 🏗️ Framework Architecture

The Handlr framework is built on these principles:

- **🚀 High Performance**: Switch expression-based dispatcher eliminates reflection overhead
- **🎯 Developer Friendly**: Normal classes implementing standard interfaces - no partial classes!
- **🤖 Source Generator**: Automatic discovery with compile-time type safety
- **📚 Example-Based Learning**: Instead of providing framework implementations, we provide clear examples you can adapt
- **🔄 Pipeline Architecture**: Behaviors wrap around handlers in a configurable pipeline
- **🏷️ Interface-Driven**: Commands and queries implement marker interfaces to opt into specific behaviors

## 📋 Available Behavior Examples

### 1. ValidationBehaviorExample.cs
**Purpose**: Demonstrates request validation before processing

**Key Features**:
- `IValidatable` interface for commands/queries that need validation
- `ValidationResult` class with error collection
- Automatic validation execution before handler
- Clear error reporting

**Usage Pattern**:
```csharp
public class CreateUserCommand : IValidatable
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    public ValidationResult Validate()
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(Name))
            result.AddError("Name is required");
            
        if (!Email.Contains("@"))
            result.AddError("Invalid email format");
            
        return result;
    }
}
```

### 2. LoggingBehaviorExample.cs
**Purpose**: Shows comprehensive request/response logging with performance tracking

**Key Features**:
- Correlation ID tracking across requests
- Request and response logging
- Performance timing with Stopwatch
- Exception logging with full details
- Structured logging format

**Usage Pattern**:
```csharp
// Automatically logs all requests/responses
// No interface required - works with any command/query
services.AddScoped<IPipelineBehavior<MyCommand, Result>, LoggingBehaviorExample<MyCommand, Result>>();
```

### 3. CachingBehaviorExample.cs
**Purpose**: Demonstrates query result caching for improved performance

**Key Features**:
- `ICacheable` interface for queries that support caching
- Configurable cache duration
- Custom cache key generation
- Thread-safe in-memory cache implementation
- Cache hit/miss tracking

**Usage Pattern**:
```csharp
public class GetUserQuery : ICacheable
{
    public int UserId { get; set; }
    
    public string GetCacheKey() => $"user:{UserId}";
    public TimeSpan GetCacheDuration() => TimeSpan.FromMinutes(5);
}
```

### 4. AuthorizationBehaviorExample.cs
**Purpose**: Shows how to implement security and permission checking

**Key Features**:
- `IRequireAuthorization` interface for protected operations
- Permission-based authorization
- Resource-based authorization support
- Integration points for auth services
- Clear access denied handling

**Usage Pattern**:
```csharp
public class DeleteUserCommand : IRequireAuthorization
{
    public int UserId { get; set; }
    
    public string RequiredPermission => "User.Delete";
    public string? ResourceId => UserId.ToString(); // For resource-based auth
}
```

### 5. RetryBehaviorExample.cs
**Purpose**: Demonstrates resilient handling of transient failures

**Key Features**:
- `IRetryable` interface for operations that can be retried
- Configurable retry count and delay
- Exponential backoff support
- Smart exception filtering
- Comprehensive retry logging

**Usage Pattern**:
```csharp
public class CallExternalApiCommand : IRetryable
{
    public string ApiEndpoint { get; set; } = string.Empty;
    
    public int MaxRetries => 3;
    public int RetryDelayMs => 1000;
    public bool UseExponentialBackoff => true;
}
```

### 6. MetricsBehaviorExample.cs
**Purpose**: Shows comprehensive metrics collection and performance monitoring

**Key Features**:
- `IMetricsEnabled` interface for tracked operations
- Performance timing and categorization
- Success/failure rate tracking
- Custom tags and dimensions
- Integration patterns for monitoring systems

**Usage Pattern**:
```csharp
public class ProcessOrderCommand : IMetricsEnabled
{
    public string OrderId { get; set; } = string.Empty;
    
    public string OperationName => "order.process";
    public Dictionary<string, string> MetricsTags => new()
    {
        { "order_id", OrderId },
        { "environment", "production" }
    };
}
```

## 🔧 Implementation Patterns

### 1. Behavior Registration
```csharp
// Register behaviors in dependency injection container
services.AddScoped<IPipelineBehavior<MyCommand, Result>, ValidationBehaviorExample<MyCommand, Result>>();
services.AddScoped<IPipelineBehavior<MyCommand, Result>, LoggingBehaviorExample<MyCommand, Result>>();
services.AddScoped<IPipelineBehavior<MyCommand, Result>, AuthorizationBehaviorExample<MyCommand, Result>>();
```

### 2. Execution Order
Behaviors execute in registration order:
1. **Authorization** - Check permissions first
2. **Validation** - Validate request structure and business rules
3. **Logging** - Log request details and performance
4. **Metrics** - Collect operational metrics
5. **Caching** - Check cache for queries
6. **Retry** - Handle transient failures
7. **Handler** - Execute business logic

### 3. Conditional Application
Behaviors only apply when the request implements the required interface:
- Commands implementing `IValidatable` get validation
- Queries implementing `ICacheable` get caching
- Requests implementing `IRequireAuthorization` get auth checks

## 🚀 Getting Started

1. **Choose Relevant Behaviors**: Pick the examples that match your needs
2. **Copy and Adapt**: Use these examples as starting points for your implementation
3. **Implement Interfaces**: Make your commands/queries implement the behavior interfaces
4. **Register in DI**: Add behavior registrations to your service collection
5. **Configure Order**: Register behaviors in the order you want them to execute

## 🔍 Example Command with Multiple Behaviors

```csharp
public class CreateOrderCommand : IValidatable, IRequireAuthorization, IMetricsEnabled
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    
    // IValidatable implementation
    public ValidationResult Validate()
    {
        var result = new ValidationResult();
        if (string.IsNullOrEmpty(ProductId)) result.AddError("ProductId required");
        if (Quantity <= 0) result.AddError("Quantity must be positive");
        if (string.IsNullOrEmpty(CustomerId)) result.AddError("CustomerId required");
        return result;
    }
    
    // IRequireAuthorization implementation
    public string RequiredPermission => "Order.Create";
    public string? ResourceId => CustomerId; // Customer-specific authorization
    
    // IMetricsEnabled implementation
    public string OperationName => "order.create";
    public Dictionary<string, string> MetricsTags => new()
    {
        { "product_id", ProductId },
        { "customer_id", CustomerId },
        { "quantity", Quantity.ToString() }
    };
}
```

## 💡 Best Practices

1. **Keep Behaviors Focused**: Each behavior should handle one cross-cutting concern
2. **Use Interfaces Wisely**: Only implement behavior interfaces when you need that behavior
3. **Handle Exceptions Gracefully**: Behaviors should fail fast and provide clear error messages
4. **Consider Performance**: Behaviors add overhead, so optimize for your use cases
5. **Make Behaviors Configurable**: Use options patterns for behavior configuration
6. **Test Behaviors Independently**: Unit test each behavior with mock handlers

## 🔄 Integration with Source Generator

The source generator will:
- **⚡ Generate high-performance dispatcher** using switch expressions instead of reflection
- **🔍 Automatically discover commands, queries, and handlers** with compile-time type safety
- **📦 Generate registration code for handlers** with direct method calls
- **🛠️ Provide extension methods for easy DI setup** with performance
- **🎯 Support normal class patterns** - just implement `ICommandHandler<T,R>` or `IQueryHandler<T,R>`

Behaviors are registered manually to give you full control over:
- Which behaviors apply to which requests
- The order of behavior execution
- Behavior-specific configuration

This approach provides maximum flexibility while maintaining the benefits of automatic code generation for the core CQRS patterns with superior performance.
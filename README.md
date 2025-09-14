<div align="center">
  <img src="https://raw.githubusercontent.com/sodiqyekeen/handlr/main/assets/logos/handlr-nuget-icon.svg" alt="Handlr Logo" width="128" height="128" />
  
  # Handlr - Modern CQRS Framework with Source Generator
  
  *CQRS Made Simple*
</div>

<div align="center">

[![NuGet - Handlr.Abstractions](https://img.shields.io/nuget/v/Handlr.Abstractions.svg?label=Handlr.Abstractions)](https://www.nuget.org/packages/Handlr.Abstractions/)
[![NuGet - Handlr.SourceGenerator](https://img.shields.io/nuget/v/Handlr.SourceGenerator.svg?label=Handlr.SourceGenerator)](https://www.nuget.org/packages/Handlr.SourceGenerator/)
[![Continuous Integration](https://github.com/sodiqyekeen/handlr/workflows/Continuous%20Integration/badge.svg)](https://github.com/sodiqyekeen/handlr/actions/workflows/ci.yml)
[![Release](https://github.com/sodiqyekeen/handlr/workflows/Release/badge.svg)](https://github.com/sodiqyekeen/handlr/actions/workflows/release.yml)
[![Security Scan](https://github.com/sodiqyekeen/handlr/workflows/Security%20Scan/badge.svg)](https://github.com/sodiqyekeen/handlr/actions/workflows/security.yml)
[![Documentation](https://github.com/sodiqyekeen/handlr/workflows/Documentation/badge.svg)](https://github.com/sodiqyekeen/handlr/actions/workflows/docs.yml)

</div>

A lightweight CQRS (Command Query Responsibility Segregation) framework for .NET that provides the infrastructure for building scalable applications with automatic code generation and extensible pipeline behavior support.

## 📦 What's Included

### Core Package (`Handlr.Abstractions`)
- 🎯 **CQRS Interfaces**: `ICommand<T>`, `IQuery<T>`, `ICommandHandler<T>`, `IQueryHandler<T>`
- 🔄 **Pipeline Infrastructure**: `IPipelineBehavior<TRequest, TResponse>` for cross-cutting concerns
- 📋 **Result Pattern Support**: Flexible return types including `Result<T>` patterns
- 🏷️ **Marker Interfaces**: For opt-in behavior patterns (e.g., `IValidatable`, `ICacheable`)

### Source Generator (`Handlr.SourceGenerator`)
- 🤖 **Automatic Registration**: Discovers and registers all handlers
- 🔧 **Boilerplate Generation**: Eliminates manual configuration
- ⚡ **Compile-time Safety**: Type-safe handler discovery

### Samples & Examples
- 📚 **Implementation Patterns**: Real-world examples of pipeline behaviors
- 🎨 **Best Practices**: Recommended approaches for common scenarios
- 🚀 **Getting Started Templates**: Ready-to-use patterns for your application

## 🚀 Features

- **Source Generator Powered**: Automatic discovery and registration of commands, queries, and handlers
- **Pipeline Behavior Support**: Infrastructure and examples for implementing cross-cutting concerns
- **Type-Safe**: Strong typing with compile-time validation
- **Flexible Results**: Support for any return type including `Result<T>` pattern
- **Dependency Injection Ready**: Built for modern .NET DI containers
- **Example-Driven**: Rich examples and templates for rapid development

## 📋 Quick Start

### 1. Install Packages
```xml
<PackageReference Include="Handlr.Abstractions" />
<PackageReference Include="Handlr.SourceGenerator" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

### 2. Define Commands and Queries
```csharp
// Command
public record CreateUserCommand : ICommand<Result<User>>
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public IDictionary<string, object>? Metadata { get; init; } = new Dictionary<string, object>();
}

// Query
public record GetUserQuery : IQuery<Result<User>>
{
    public int UserId { get; init; }
    public string? CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public IDictionary<string, object>? Metadata { get; init; } = new Dictionary<string, object>();
}
```

### 3. Implement Handlers (Partial Classes)
```csharp
public partial class CreateUserCommandHandler
{
    public partial async Task<Result<User>> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // Your business logic here
        var user = new User(command.Name, command.Email);
        return Result<User>.Success(user);
    }
}
```

### 4. Register and Use
```csharp
// The source generator automatically creates registration extensions
services.AddHandlr();

// Send commands and queries
var result = await handler.Send(new CreateUserCommand { Name = "John", Email = "john@example.com" });
```

## 🎯 Pipeline Behavior Examples

The framework provides the infrastructure for pipeline behaviors and includes comprehensive examples to help you implement cross-cutting concerns:

### Example Behavior Patterns

| Example Pattern | Purpose | Marker Interface | Sample Implementation |
|----------------|---------|------------------|----------------------|
| **Validation** | Request validation | `IValidatable` | Input validation with detailed error reporting |
| **Logging** | Request/response logging | *All requests* | Correlation IDs, performance tracking |
| **Caching** | Query result caching | `ICacheable` | Configurable cache duration and keys |
| **Authorization** | Permission checking | `IRequireAuthorization` | Role/permission-based access control |
| **Retry** | Transient failure handling | `IRetryable` | Exponential backoff, smart retry logic |
| **Metrics** | Performance monitoring | `IMetricsEnabled` | Custom tags, success/failure tracking |

> **Note**: These are example patterns provided in the samples folder. You need to implement the actual behavior classes according to your application's needs.

### Example: Command with Multiple Behaviors
```csharp
public record CreateOrderCommand : ICommand<Result<Order>>, IValidatable, IRequireAuthorization, IMetricsEnabled
{
    public string ProductId { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public string CustomerId { get; init; } = string.Empty;
    
    // IValidatable implementation
    public ValidationResult Validate()
    {
        var result = new ValidationResult();
        if (string.IsNullOrEmpty(ProductId)) result.AddError("ProductId required");
        if (Quantity <= 0) result.AddError("Quantity must be positive");
        return result;
    }
    
    // IRequireAuthorization implementation
    public string RequiredPermission => "Order.Create";
    public string? ResourceId => CustomerId;
    
    // IMetricsEnabled implementation
    public string OperationName => "order.create";
    public Dictionary<string, string> MetricsTags => new()
    {
        { "product_id", ProductId },
        { "quantity", Quantity.ToString() }
    };
    
    // Required ICommand properties
    public string? CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public IDictionary<string, object>? Metadata { get; init; } = new Dictionary<string, object>();
}
```

## 🔧 Pipeline Behavior Registration

Handlr provides flexible pipeline behavior registration using standard .NET DI patterns. You can choose between **global** and **selective** registration approaches:

### 🌍 Global Registration (Recommended)

Apply behaviors to **all** commands and queries automatically:

```csharp
// ✅ Global registration - applies to ALL commands and queries
services.AddHandlr();

// Register behaviors globally using generic type registration
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviorExample<,>));
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviorExample<,>));
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(MetricsBehaviorExample<,>));

// Execution order follows registration order:
// 1. LoggingBehavior → 2. ValidationBehavior → 3. MetricsBehavior → 4. Handler
```

**Benefits:**
- ✅ **Single registration** per behavior type
- ✅ **Automatic coverage** for all commands/queries  
- ✅ **Easy maintenance** - add new commands without extra registration
- ✅ **Standard .NET DI** - familiar pattern for all developers

### 🎯 Selective Registration

Apply behaviors only to **specific** command/query combinations:

```csharp
// ✅ Selective registration - fine-grained control
services.AddHandlr();

// Register for specific command-response combinations
services.AddScoped<IPipelineBehavior<CreateOrderCommand, Result<Order>>, AuthorizationBehaviorExample<CreateOrderCommand, Result<Order>>>();
services.AddScoped<IPipelineBehavior<CreateOrderCommand, Result<Order>>, ValidationBehaviorExample<CreateOrderCommand, Result<Order>>>();

// Different behaviors for different commands
services.AddScoped<IPipelineBehavior<GetUserQuery, Result<User>>, CachingBehaviorExample<GetUserQuery, Result<User>>>();
services.AddScoped<IPipelineBehavior<GetUserQuery, Result<User>>, LoggingBehaviorExample<GetUserQuery, Result<User>>>();
```

**Benefits:**
- ✅ **Precise control** over which behaviors apply where
- ✅ **Performance optimization** - only run necessary behaviors
- ✅ **Different behavior chains** for different command types

### 🎭 Mixed Registration

Combine both approaches for maximum flexibility:

```csharp
services.AddHandlr();

// Global behaviors for cross-cutting concerns
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviorExample<,>));
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(MetricsBehaviorExample<,>));

// Selective behaviors for specific needs
services.AddScoped<IPipelineBehavior<SecurityCommand, Result>, AuthorizationBehaviorExample<SecurityCommand, Result>>();
services.AddScoped<IPipelineBehavior<CachedQuery, Result<Data>>, CachingBehaviorExample<CachedQuery, Result<Data>>>();
```

### 🔄 Execution Order

Behaviors execute in **registration order**:

```csharp
// Execution order: Auth → Validation → Logging → Metrics → Handler
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviorExample<,>));  // 1st
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviorExample<,>));     // 2nd
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviorExample<,>));        // 3rd
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(MetricsBehaviorExample<,>));        // 4th
```

> **💡 Best Practice**: Register cross-cutting concerns (logging, metrics) globally, and specific behaviors (authorization, caching) selectively.

## 📚 Documentation

- **[Pipeline Behaviors Guide](https://github.com/sodiqyekeen/handlr/blob/main/README-BEHAVIORS.md)** - Comprehensive behavior implementation guide
- **[Behavior Examples](https://github.com/sodiqyekeen/handlr/tree/main/samples/SampleConsoleApp/Examples)** - Complete examples with explanations

## 🏗️ Project Structure

```
├── src/
│   ├── Handlr.Abstractions/          # Core interfaces and abstractions
│   ├── Handlr.SourceGenerator/        # Source generator implementation
│   └── Handlr.Extensions/             # Additional extensions
├── samples/
│   ├── SampleConsoleApp/              # Console app with behavior examples
│   │   └── Examples/                  # Comprehensive behavior examples
│   └── SampleWebApi/                  # Web API example
└── tests/                             # Unit and integration tests
```

## 🎯 Architecture Principles

1. **Example-Driven Development**: Framework provides examples, not implementations
2. **Source Generator First**: Automatic code generation for boilerplate
3. **Interface-Based Behaviors**: Opt-in behaviors through marker interfaces
4. **Pipeline Architecture**: Composable behaviors with clear execution order
5. **Type Safety**: Compile-time validation and strong typing

## 🚀 Getting Started

1. **Explore Examples**: Check out `samples/SampleConsoleApp/Examples/` for comprehensive behavior examples
2. **Copy and Adapt**: Use examples as starting points for your implementations
3. **Understand Patterns**: Review the [Pipeline Behaviors Guide](https://github.com/sodiqyekeen/handlr/blob/main/README-BEHAVIORS.md)
4. **Build Your App**: Apply the patterns to your specific use cases

## 🔧 What You Can Build

With Handlr's pipeline behavior infrastructure, you can implement:

- **Conditional Behaviors**: Apply behaviors based on environment or configuration
- **Behavior Composition**: Combine multiple concerns in single behaviors  
- **Dynamic Selection**: Choose behaviors based on request properties
- **Performance Monitoring**: Custom metrics and performance tracking
- **Error Handling**: Comprehensive exception handling patterns

> **Note**: These are capabilities you can implement using the provided infrastructure and example patterns.

## 📖 Learn More

- Run `dotnet run` in `samples/SampleConsoleApp` to see the framework in action
- Explore behavior examples in `samples/SampleConsoleApp/Examples/`
- Read the comprehensive [behaviors documentation](https://github.com/sodiqyekeen/handlr/blob/main/README-BEHAVIORS.md)

## Samples

Check out the [samples](samples/) directory for complete working examples:
- [Web API Sample](samples/SampleWebApi/) - ASP.NET Core Web API
- [Console App Sample](samples/SampleConsoleApp/) - Console application with comprehensive behavior examples

## Contributing

We welcome contributions! Please see our [Contributing Guide](https://github.com/sodiqyekeen/handlr/blob/main/CONTRIBUTING.md) for details.

## 🔄 CI/CD Pipeline

This project uses a comprehensive CI/CD pipeline with:

- **Continuous Integration**: Automated testing on .NET 9.0
- **Automated Releases**: Semantic versioning with auto-generated changelogs
- **Security Scanning**: Vulnerability detection and secret scanning
- **Documentation**: Auto-generated API docs deployed to GitHub Pages

See [CI/CD Pipeline Documentation](https://github.com/sodiqyekeen/handlr/blob/main/.github/CI-CD-PIPELINE.md) for complete details.

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/sodiqyekeen/handlr/blob/main/LICENSE) file for details.

## Support

- 📖 [Documentation](https://github.com/sodiqyekeen/handlr/tree/main/docs)
- 💬 [Discussions](https://github.com/sodiqyekeen/handlr/discussions)
- 🐛 [Issues](https://github.com/sodiqyekeen/handlr/issues)

---

The Handlr framework makes it easy to build maintainable, scalable applications with clean CQRS patterns and powerful cross-cutting concerns.

Built with ❤️ by [Sodiq Yekeen](https://github.com/sodiqyekeen)
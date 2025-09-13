<div align="center">
  <img src="assets/logos/handlr-logo.svg" alt="Handlr Logo" width="200" height="200" />
  
  # Handlr - Modern CQRS Framework with Source Generator
  
  *CQRS Made Simple*
</div>

<div align="center">

[![NuGet](https://img.shields.io/nuget/v/Handlr.svg)](https://www.nuget.org/packages/Handlr.Abstractions/)
[![NuGet](https://img.shields.io/nuget/v/Handlr.svg)](https://www.nuget.org/packages/Handlr.SourceGenerator/)
[![CI](https://github.com/sodiqyekeen/handlr/workflows/CI/badge.svg)](https://github.com/sodiqyekeen/handlr/actions/workflows/ci.yml)
[![Release](https://github.com/sodiqyekeen/handlr/workflows/Release/badge.svg)](https://github.com/sodiqyekeen/handlr/actions/workflows/release.yml)
[![Security](https://github.com/sodiqyekeen/handlr/workflows/Security/badge.svg)](https://github.com/sodiqyekeen/handlr/actions/workflows/security.yml)
[![Docs](https://github.com/sodiqyekeen/handlr/workflows/Documentation/badge.svg)](https://github.com/sodiqyekeen/handlr/actions/workflows/docs.yml)

</div>

A comprehensive CQRS (Command Query Responsibility Segregation) framework for .NET with automatic code generation and pipeline behavior support.

## 🚀 Features

- **Source Generator Powered**: Automatic discovery and registration of commands, queries, and handlers
- **Pipeline Behaviors**: Comprehensive cross-cutting concerns (validation, logging, caching, authorization, retry, metrics)
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
var result = await mediator.Send(new CreateUserCommand { Name = "John", Email = "john@example.com" });
```

## 🎯 Pipeline Behaviors

The framework provides comprehensive examples for implementing cross-cutting concerns:

### Available Behavior Examples

| Behavior | Purpose | Interface | Example Usage |
|----------|---------|-----------|---------------|
| **Validation** | Request validation | `IValidatable` | Input validation with detailed error reporting |
| **Logging** | Request/response logging | *All requests* | Correlation IDs, performance tracking |
| **Caching** | Query result caching | `ICacheable` | Configurable cache duration and keys |
| **Authorization** | Permission checking | `IRequireAuthorization` | Role/permission-based access control |
| **Retry** | Transient failure handling | `IRetryable` | Exponential backoff, smart retry logic |
| **Metrics** | Performance monitoring | `IMetricsEnabled` | Custom tags, success/failure tracking |

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

### Behavior Registration
```csharp
// Register behaviors in desired execution order
services.AddScoped<IPipelineBehavior<CreateOrderCommand, Result<Order>>, AuthorizationBehaviorExample<CreateOrderCommand, Result<Order>>>();
services.AddScoped<IPipelineBehavior<CreateOrderCommand, Result<Order>>, ValidationBehaviorExample<CreateOrderCommand, Result<Order>>>();
services.AddScoped<IPipelineBehavior<CreateOrderCommand, Result<Order>>, LoggingBehaviorExample<CreateOrderCommand, Result<Order>>>();
services.AddScoped<IPipelineBehavior<CreateOrderCommand, Result<Order>>, MetricsBehaviorExample<CreateOrderCommand, Result<Order>>>();
```

## 📚 Documentation

- **[Pipeline Behaviors Guide](./README-BEHAVIORS.md)** - Comprehensive behavior implementation guide
- **[Behavior Examples](./samples/SampleConsoleApp/Examples/README.md)** - Complete examples with explanations

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
3. **Understand Patterns**: Review the [Pipeline Behaviors Guide](./README-BEHAVIORS.md)
4. **Build Your App**: Apply the patterns to your specific use cases

## 🔧 Advanced Features

- **Conditional Behaviors**: Apply behaviors based on environment or configuration
- **Behavior Composition**: Combine multiple concerns in single behaviors
- **Dynamic Selection**: Choose behaviors based on request properties
- **Performance Monitoring**: Built-in metrics and performance tracking
- **Error Handling**: Comprehensive exception handling patterns

## 📖 Learn More

- Run `dotnet run` in `samples/SampleConsoleApp` to see the framework in action
- Explore behavior examples in `samples/SampleConsoleApp/Examples/`
- Read the comprehensive [behaviors documentation](./README-BEHAVIORS.md)

## Samples

Check out the [samples](samples/) directory for complete working examples:
- [Web API Sample](samples/SampleWebApi/) - ASP.NET Core Web API
- [Console App Sample](samples/SampleConsoleApp/) - Console application with comprehensive behavior examples

## Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

## 🔄 CI/CD Pipeline

This project uses a comprehensive CI/CD pipeline with:

- **Continuous Integration**: Automated testing on .NET 9.0
- **Automated Releases**: Semantic versioning with auto-generated changelogs
- **Security Scanning**: Vulnerability detection and secret scanning
- **Documentation**: Auto-generated API docs deployed to GitHub Pages

See [CI/CD Pipeline Documentation](.github/CI-CD-PIPELINE.md) for complete details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- 📖 [Documentation](docs/)
- 💬 [Discussions](https://github.com/sodiqyekeen/handlr/discussions)
- 🐛 [Issues](https://github.com/sodiqyekeen/handlr/issues)

---

The Handlr framework makes it easy to build maintainable, scalable applications with clean CQRS patterns and powerful cross-cutting concerns.

Built with ❤️ by [Sodiq Yekeen](https://github.com/sodiqyekeen)
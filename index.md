# Handlr CQRS Framework

<div align="center">
  <img src="assets/logos/handlr-logo.svg" alt="Handlr Logo" width="120" height="120" />
  <br><br>
  <strong>A high-performance, developer-friendly CQRS framework for .NET with switch expression-based dispatching</strong>
</div>

## 🚀 Quick Start

```csharp
// Install the NuGet packages
dotnet add package Handlr.Abstractions
dotnet add package Handlr.SourceGenerator

// Define a command using BaseCommand<T>
public record CreateUserCommand : BaseCommand<User>
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

// Implement handler with normal class - no partial classes!
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, User>
{
    public async Task<User> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // Your business logic here
        return new User(command.Name, command.Email);
    }
}

// Register and use with high-performance dispatcher
services.AddHandlr();
var result = await dispatcher.SendAsync(new CreateUserCommand { Name = "John", Email = "john@example.com" });
```

## 📚 Documentation

- **[Getting Started Guide](docs/index.md)** - Complete setup and usage guide
- **[API Reference](api/index.md)** - Detailed API documentation
- **[Sample Projects](https://github.com/sodiqyekeen/handlr/tree/main/samples)** - Working examples

## 🛠️ Features

- ✅ **⚡ High Performance** - Switch expression dispatcher eliminates reflection overhead
- ✅ **🚀 Developer Friendly** - Normal classes implementing standard interfaces, no partial classes!
- ✅ **🤖 Source Generator** - Automatic discovery with compile-time type safety
- ✅ **🏗️ CQRS Pattern** - Clean separation of commands and queries  
- ✅ **🔄 Pipeline Behaviors** - Validation, logging, caching, and more
- ✅ **🏗️ Dependency Injection** - Built-in DI container support
- ✅ **🛡️ Type Safety** - Compile-time validation with IntelliSense
- ✅ **🚀 .NET 9.0** - Latest .NET features and performance

## 📦 Packages

| Package | Version | Description |
|---------|---------|-------------|
| `Handlr.Abstractions` | [![NuGet](https://img.shields.io/nuget/v/Handlr.Abstractions.svg)](https://www.nuget.org/packages/Handlr.Abstractions/) | Core abstractions, interfaces, and base classes |
| `Handlr.SourceGenerator` | [![NuGet](https://img.shields.io/nuget/v/Handlr.SourceGenerator.svg)](https://www.nuget.org/packages/Handlr.SourceGenerator/) | High-performance source generator with switch expression dispatcher |

## 🤝 Contributing

We welcome contributions! Visit our [GitHub repository](https://github.com/sodiqyekeen/handlr) to get started.

---

*Built with ❤️ for the .NET community*
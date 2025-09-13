# Installation Guide

Learn how to install and set up the Handlr CQRS Framework in your .NET 9.0 project.

## Prerequisites

- .NET 9.0 SDK or later
- Visual Studio 2022 or VS Code
- Basic understanding of CQRS patterns

## Package Installation

### Using Package Manager Console

```powershell
Install-Package Handlr.Abstractions
Install-Package Handlr.SourceGenerator
```

### Using .NET CLI

```bash
dotnet add package Handlr.Abstractions
dotnet add package Handlr.SourceGenerator
```

### Using PackageReference

Add these to your `.csproj` file:

```xml
<PackageReference Include="Handlr.Abstractions" Version="1.0.0" />
<PackageReference Include="Handlr.SourceGenerator" Version="1.0.0" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

## Quick Setup

1. **Create your first command:**
   ```csharp
   public record CreateUserCommand(string Name, string Email) : ICommand<User>;
   ```

2. **Register services** in your DI container:
   ```csharp
   services.AddHandlr();
   ```

3. **Use the mediator:**
   ```csharp
   var user = await mediator.Send(new CreateUserCommand("John", "john@example.com"));
   ```

## Next Steps

- [View complete examples](https://github.com/sodiqyekeen/handlr/tree/main/samples)
- [Browse API documentation](../api/index.md)
- [Learn about pipeline behaviors](https://github.com/sodiqyekeen/handlr/blob/main/README-BEHAVIORS.md)

## Troubleshooting

### Source Generator Not Working
If the source generator isn't generating handlers:

1. **Restart your IDE** after installing the packages
2. **Clean and rebuild** your solution
3. **Check that your commands/queries implement the correct interfaces**

### Build Errors
- Ensure you're targeting .NET 9.0 or later
- Verify all packages are compatible versions
- Check for conflicting MediatR or similar packages

## Support

If you encounter issues:
- [Report bugs on GitHub](https://github.com/sodiqyekeen/handlr/issues)
- [View documentation](https://sodiqyekeen.github.io/handlr/)
- [Check sample projects](https://github.com/sodiqyekeen/handlr/tree/main/samples)
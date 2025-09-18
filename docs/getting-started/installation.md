# Installation Guide

Learn how to install and set up the enhanced Handlr CQRS Framework in your .NET 9.0 project. The enhanced framework features switch expression-based dispatching for superior performance and a simplified developer experience with normal classes.

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
<PackageReference Include="Handlr.Abstractions" Version="1.0.4" />
<PackageReference Include="Handlr.SourceGenerator" Version="1.0.4" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

> **Note**: The enhanced source generator now produces high-performance switch expression-based dispatchers with compile-time type safety.

## Quick Setup

1. **Create your first command using BaseCommand<T>:**
   ```csharp
   public record CreateUserCommand : BaseCommand<User>
   {
       public string Name { get; init; } = string.Empty;
       public string Email { get; init; } = string.Empty;
   }
   ```

2. **Implement handler with normal class (no partial classes!):**
   ```csharp
   public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, User>
   {
       public async Task<User> Handle(CreateUserCommand command, CancellationToken cancellationToken)
       {
           // Your business logic here
           return new User(command.Name, command.Email);
       }
   }
   ```

3. **Register services** - Enhanced source generator automatically discovers handlers:
   ```csharp
   services.AddHandlr(); // Registers high-performance dispatcher
   ```

4. **Use the dispatcher:**
   ```csharp
   public class MyController
   {
       private readonly IHandlrDispatcher _dispatcher;
       
       public MyController(IHandlrDispatcher dispatcher) => _dispatcher = dispatcher;
       
       public async Task<User> CreateUser(CreateUserRequest request)
       {
           return await _dispatcher.SendAsync(new CreateUserCommand 
           { 
               Name = request.Name, 
               Email = request.Email 
           });
       }
   }
   ```

## Next Steps

- [View complete examples](https://github.com/sodiqyekeen/handlr/tree/main/samples)
- [Browse API documentation](../api/index.md)
- [Learn about pipeline behaviors](https://github.com/sodiqyekeen/handlr/blob/main/README-BEHAVIORS.md)

## Troubleshooting

### Enhanced Source Generator Not Working
If the enhanced source generator isn't generating the dispatcher:

1. **Restart your IDE** after installing the packages
2. **Clean and rebuild** your solution
3. **Check that your commands/queries inherit from BaseCommand<T> or BaseQuery<T>**
4. **Verify handlers implement ICommandHandler<T,R> or IQueryHandler<T,R>**
5. **Check generated files** in `obj/Debug/net9.0/generated/` folder

### Build Errors
- Ensure you're targeting .NET 9.0 or later
- Verify all packages are compatible versions
- Check for conflicting MediatR or similar packages
- Ensure handlers are normal classes, not partial classes

### Performance Verification
To verify the enhanced performance:
- Check that generated dispatcher uses switch expressions
- Look for `GeneratedHandlrDispatcher.g.cs` in generated files
- No reflection-based calls should appear in generated code

## Support

If you encounter issues:
- [Report bugs on GitHub](https://github.com/sodiqyekeen/handlr/issues)
- [View documentation](https://sodiqyekeen.github.io/handlr/)
- [Check sample projects](https://github.com/sodiqyekeen/handlr/tree/main/samples)
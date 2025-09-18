# Handlr.Extensions.FluentValidation

[![NuGet](https://img.shields.io/nuget/v/Handlr.Extensions.FluentValidation.svg)](https://www.nuget.org/packages/Handlr.Extensions.FluentValidation/)
[![Downloads](https://img.shields.io/nuget/dt/Handlr.Extensions.FluentValidation.svg)](https://www.nuget.org/packages/Handlr.Extensions.FluentValidation/)

FluentValidation support for the Handlr CQRS framework. This extension provides automatic validation pipeline behaviors and seamless integration with FluentValidation validators.

## ✨ Features

- 🔍 **Automatic Validator Discovery**: Automatically finds and registers all FluentValidation validators
- 🚀 **Pipeline Integration**: Validation executes automatically before your handlers
- 📋 **Detailed Error Reporting**: Comprehensive validation error information with property names
- 🎯 **Zero Configuration**: Works out of the box with standard FluentValidation patterns
- 🛡️ **Type Safety**: Compile-time validation support with IntelliSense
- ⚡ **Performance Optimized**: Efficient validator resolution and caching

## 📦 Installation

### Package Manager Console
```powershell
Install-Package Handlr.Extensions.FluentValidation
```

### .NET CLI
```bash
dotnet add package Handlr.Extensions.FluentValidation
```

### PackageReference
```xml
<PackageReference Include="Handlr.Extensions.FluentValidation" />
```

## 🚀 Quick Start

### 1. Install Prerequisites
You need the core Handlr packages and FluentValidation:

```xml
<PackageReference Include="Handlr.Abstractions" />
<PackageReference Include="Handlr.SourceGenerator" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
<PackageReference Include="Handlr.Extensions.FluentValidation" />
```

### 2. Register Services
```csharp
using Handlr.Extensions.FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Register core Handlr services
builder.Services.AddHandlr();

// Add FluentValidation extension
builder.Services.AddHandlrFluentValidation();

var app = builder.Build();
```

### 3. Create Commands/Queries
```csharp
public record CreateUserCommand : ICommand<Result<User>>
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int Age { get; init; }
}

public record GetUsersByFilterQuery : IQuery<Result<IEnumerable<User>>>
{
    public string? NameFilter { get; init; }
    public int? MinAge { get; init; }
    public int PageSize { get; init; } = 10;
}
```

### 4. Create Validators
```csharp
using FluentValidation;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .Length(2, 50)
            .WithMessage("Name must be between 2 and 50 characters");
            
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address");
            
        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18)
            .WithMessage("User must be at least 18 years old")
            .LessThanOrEqualTo(120)
            .WithMessage("Invalid age");
    }
}

public class GetUsersByFilterQueryValidator : AbstractValidator<GetUsersByFilterQuery>
{
    public GetUsersByFilterQueryValidator()
    {
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size cannot exceed 100");
            
        RuleFor(x => x.MinAge)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Minimum age cannot be negative")
            .When(x => x.MinAge.HasValue);
    }
}
```

### 5. Implement Handlers
```csharp
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Result<User>>
{
    public async Task<Result<User>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // Validation already executed automatically!
        // Your business logic here
        var user = new User
        {
            Name = command.Name,
            Email = command.Email,
            Age = command.Age
        };
        
        // Save user logic...
        
        return Result<User>.Success(user);
    }
}
```

### 6. Use in Controllers
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IHandlrDispatcher _dispatcher;
    
    public UsersController(IHandlrDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserCommand command)
    {
        try
        {
            var result = await _dispatcher.SendAsync(command);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }
        catch (ValidationException ex)
        {
            // Automatic validation error handling
            var errors = ex.Errors.ToDictionary(
                error => error.PropertyName, 
                error => error.ErrorMessage
            );
            return BadRequest(new { Errors = errors });
        }
    }
}
```

## 🔧 Advanced Usage

### Custom Validation Behavior

You can customize the validation behavior by implementing your own:

```csharp
public class CustomFluentValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<CustomFluentValidationBehavior<TRequest, TResponse>> _logger;
    private readonly IServiceProvider _serviceProvider;
    
    public CustomFluentValidationBehavior(
        ILogger<CustomFluentValidationBehavior<TRequest, TResponse>> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Validating request {RequestType}", typeof(TRequest).Name);
        
        var validators = _serviceProvider.GetServices<IValidator<TRequest>>();
        
        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );
            
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();
                
            if (failures.Any())
            {
                _logger.LogWarning("Validation failed for {RequestType} with {ErrorCount} errors",
                    typeof(TRequest).Name, failures.Count);
                    
                throw new ValidationException(failures);
            }
        }
        
        _logger.LogDebug("Validation passed for {RequestType}", typeof(TRequest).Name);
        return await next();
    }
}

// Register your custom behavior instead of the default
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CustomFluentValidationBehavior<,>));
```

### Conditional Validation

Apply validation only to specific commands or queries:

```csharp
public class ConditionalValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IServiceProvider _serviceProvider;
    
    public ConditionalValidationBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // Only validate commands, skip queries
        if (typeof(TRequest).GetInterfaces().Any(i => 
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>)))
        {
            var validators = _serviceProvider.GetServices<IValidator<TRequest>>();
            
            if (validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);
                var validationResults = await Task.WhenAll(
                    validators.Select(v => v.ValidateAsync(context, cancellationToken))
                );
                
                var failures = validationResults
                    .SelectMany(r => r.Errors)
                    .Where(f => f != null)
                    .ToList();
                    
                if (failures.Any())
                {
                    throw new ValidationException(failures);
                }
            }
        }
        
        return await next();
    }
}
```

## ⚡ Performance Tips

1. **Validator Caching**: Validators are automatically cached by the DI container
2. **Async Validation**: Use `ValidateAsync` for database-dependent validation rules
3. **Selective Registration**: Register validators only for types that need validation
4. **Custom Context**: Use `ValidationContext` to pass additional data to validators

## 🛠️ Configuration Options

### Service Registration Options

```csharp
// Default registration
services.AddHandlrFluentValidation();

// Manual registration for more control
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(FluentValidationBehavior<,>));

// Register all validators in assembly
services.AddValidatorsFromAssembly(typeof(CreateUserCommandValidator).Assembly);
```

### Validation Context

```csharp
public class EnhancedUserValidator : AbstractValidator<CreateUserCommand>
{
    public EnhancedUserValidator(IUserRepository userRepository)
    {
        RuleFor(x => x.Email)
            .MustAsync(async (email, cancellation) =>
            {
                return !await userRepository.EmailExistsAsync(email, cancellation);
            })
            .WithMessage("Email already exists");
    }
}
```

## 📋 Error Handling

The extension throws `ValidationException` with detailed error information:

```csharp
public class ValidationError
{
    public string PropertyName { get; set; }
    public string ErrorMessage { get; set; }
}

public class ValidationException : Exception
{
    public IReadOnlyList<ValidationError> Errors { get; }
    
    public ValidationException(IEnumerable<FluentValidation.Results.ValidationFailure> failures)
        : base("One or more validation failures occurred.")
    {
        Errors = failures
            .Select(failure => new ValidationError
            {
                PropertyName = failure.PropertyName,
                ErrorMessage = failure.ErrorMessage
            })
            .ToList();
    }
}
```

## 🔍 Troubleshooting

### Common Issues

1. **Validators Not Found**: Ensure validators are registered in DI container
2. **Validation Not Running**: Check that `AddHandlrFluentValidation()` is called
3. **Multiple Validators**: All validators for a type will run - combine or use conditional logic

### Debug Logging

Enable debug logging to see validation behavior:

```json
{
  "Logging": {
    "LogLevel": {
      "Handlr.Extensions.FluentValidation": "Debug"
    }
  }
}
```

## 🔗 Related Packages

- **[Handlr.Abstractions](https://www.nuget.org/packages/Handlr.Abstractions/)**: Core CQRS interfaces
- **[Handlr.SourceGenerator](https://www.nuget.org/packages/Handlr.SourceGenerator/)**: Automatic code generation
- **[Handlr.Extensions.DataAnnotations](https://www.nuget.org/packages/Handlr.Extensions.DataAnnotations/)**: Data Annotations validation
- **[FluentValidation](https://www.nuget.org/packages/FluentValidation/)**: The underlying validation library

## 📖 Documentation

- [Main Handlr Documentation](https://github.com/sodiqyekeen/handlr)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [Pipeline Behaviors Guide](https://github.com/sodiqyekeen/handlr/blob/main/README-BEHAVIORS.md)

## 🤝 Contributing

Contributions are welcome! Please see the [main repository](https://github.com/sodiqyekeen/handlr) for contribution guidelines.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/sodiqyekeen/handlr/blob/main/LICENSE) file for details.

---

Built with ❤️ by [Sodiq Yekeen](https://github.com/sodiqyekeen) and the .NET community.
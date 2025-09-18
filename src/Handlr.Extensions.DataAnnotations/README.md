# Handlr.Extensions.DataAnnotations

[![NuGet](https://img.shields.io/nuget/v/Handlr.Extensions.DataAnnotations.svg)](https://www.nuget.org/packages/Handlr.Extensions.DataAnnotations/)
[![Downloads](https://img.shields.io/nuget/dt/Handlr.Extensions.DataAnnotations.svg)](https://www.nuget.org/packages/Handlr.Extensions.DataAnnotations/)

Data Annotations validation support for the Handlr CQRS framework. This extension provides automatic validation pipeline behaviors using System.ComponentModel.DataAnnotations - the built-in .NET validation framework.

## ✨ Features

- 📋 **Built-in Attributes**: Use standard .NET validation attributes
- 🚀 **Pipeline Integration**: Validation executes automatically before your handlers
- 🛡️ **Zero Dependencies**: Uses only System.ComponentModel.DataAnnotations
- ⚡ **Lightweight**: Minimal overhead for simple validation scenarios
- 🎯 **Familiar**: Standard .NET validation patterns
- 🔍 **Detailed Errors**: Comprehensive validation error reporting with property names
- 🔄 **Recursive Validation**: Automatically validates nested objects and collections

## 📦 Installation

### Package Manager Console
```powershell
Install-Package Handlr.Extensions.DataAnnotations
```

### .NET CLI
```bash
dotnet add package Handlr.Extensions.DataAnnotations
```

### PackageReference
```xml
<PackageReference Include="Handlr.Extensions.DataAnnotations" />
```

## 🚀 Quick Start

### 1. Install Prerequisites
You need the core Handlr packages:

```xml
<PackageReference Include="Handlr.Abstractions" />
<PackageReference Include="Handlr.SourceGenerator" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
<PackageReference Include="Handlr.Extensions.DataAnnotations" />
```

### 2. Register Services
```csharp
using Handlr.Extensions.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// Register core Handlr services
builder.Services.AddHandlr();

// Add DataAnnotations validation extension
builder.Services.AddHandlrDataAnnotations();

var app = builder.Build();
```

### 3. Annotate Commands/Queries
```csharp
using System.ComponentModel.DataAnnotations;

public record CreateUserCommand : ICommand<Result<User>>
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters")]
    public string Name { get; init; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address")]
    public string Email { get; init; } = string.Empty;
    
    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    public int Age { get; init; }
}

public record GetUsersByFilterQuery : IQuery<Result<IEnumerable<User>>>
{
    [StringLength(100, ErrorMessage = "Name filter cannot exceed 100 characters")]
    public string? NameFilter { get; init; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Minimum age cannot be negative")]
    public int? MinAge { get; init; }
    
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; init; } = 10;
}
```

### 4. Implement Handlers
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

### 5. Use in Controllers
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
        catch (DataAnnotationsValidationException ex)
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

## 📋 Available Validation Attributes

### Basic Attributes
```csharp
public record ExampleCommand : ICommand<Result>
{
    [Required]
    public string RequiredField { get; init; } = string.Empty;
    
    [StringLength(100)]
    public string LimitedString { get; init; } = string.Empty;
    
    [StringLength(50, MinimumLength = 5)]
    public string RangedString { get; init; } = string.Empty;
    
    [Range(1, 100)]
    public int NumericRange { get; init; }
    
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
    
    [Phone]
    public string PhoneNumber { get; init; } = string.Empty;
    
    [Url]
    public string Website { get; init; } = string.Empty;
    
    [CreditCard]
    public string CreditCardNumber { get; init; } = string.Empty;
}
```

### Pattern Validation
```csharp
public record PatternCommand : ICommand<Result>
{
    [RegularExpression(@"^[A-Z]+[a-zA-Z]*$", ErrorMessage = "Must start with uppercase letter")]
    public string Code { get; init; } = string.Empty;
    
    [RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "Invalid phone number format")]
    public string InternationalPhone { get; init; } = string.Empty;
}
```

### Custom Error Messages
```csharp
public record CustomMessageCommand : ICommand<Result>
{
    [Required(ErrorMessage = "User name is mandatory")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 30 characters")]
    public string Username { get; init; } = string.Empty;
    
    [Range(18, 100, ErrorMessage = "Age must be between 18 and 100 years")]
    public int Age { get; init; }
}
```

### Complex Objects
```csharp
public record CreateOrderCommand : ICommand<Result<Order>>
{
    [Required]
    [ValidateComplexType] // Validates nested object
    public CustomerInfo Customer { get; init; } = new();
    
    [Required]
    [MinLength(1, ErrorMessage = "At least one item is required")]
    public List<OrderItem> Items { get; init; } = new();
}

public class CustomerInfo
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class OrderItem
{
    [Required]
    public string ProductId { get; set; } = string.Empty;
    
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
```

## 🔧 Advanced Usage

### Custom Validation Behavior

You can customize the validation behavior:

```csharp
public class CustomDataAnnotationsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<CustomDataAnnotationsBehavior<TRequest, TResponse>> _logger;
    
    public CustomDataAnnotationsBehavior(ILogger<CustomDataAnnotationsBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }
    
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Validating {RequestType} with DataAnnotations", typeof(TRequest).Name);
        
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        
        bool isValid = Validator.TryValidateObject(
            request, 
            validationContext, 
            validationResults, 
            validateAllProperties: true
        );
        
        if (!isValid)
        {
            _logger.LogWarning("Validation failed for {RequestType} with {ErrorCount} errors",
                typeof(TRequest).Name, validationResults.Count);
                
            var errors = validationResults
                .SelectMany(vr => vr.MemberNames.Select(memberName => new ValidationError
                {
                    PropertyName = memberName,
                    ErrorMessage = vr.ErrorMessage ?? "Validation failed"
                }))
                .ToList();
                
            throw new DataAnnotationsValidationException(errors);
        }
        
        _logger.LogDebug("Validation passed for {RequestType}", typeof(TRequest).Name);
        return await next();
    }
}

// Register your custom behavior
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CustomDataAnnotationsBehavior<,>));
```

### Conditional Validation

Apply validation only to specific types:

```csharp
public class ConditionalDataAnnotationsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // Only validate if request has validation attributes
        var properties = typeof(TRequest).GetProperties();
        var hasValidationAttributes = properties.Any(p => 
            p.GetCustomAttributes(typeof(ValidationAttribute), inherit: true).Any());
            
        if (hasValidationAttributes)
        {
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            
            bool isValid = Validator.TryValidateObject(
                request, 
                validationContext, 
                validationResults, 
                validateAllProperties: true
            );
            
            if (!isValid)
            {
                var errors = validationResults
                    .SelectMany(vr => vr.MemberNames.Select(memberName => new ValidationError
                    {
                        PropertyName = memberName,
                        ErrorMessage = vr.ErrorMessage ?? "Validation failed"
                    }))
                    .ToList();
                    
                throw new DataAnnotationsValidationException(errors);
            }
        }
        
        return await next();
    }
}
```

## 🛠️ Custom Validation Attributes

Create your own validation attributes:

```csharp
public class PositiveNumberAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is int intValue)
        {
            return intValue > 0;
        }
        
        if (value is decimal decimalValue)
        {
            return decimalValue > 0;
        }
        
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
        {
            return dateValue > DateTime.Now;
        }
        
        return false;
    }
    
    public override string FormatErrorMessage(string name)
    {
        return $"{name} must be a future date";
    }
}

// Usage
public record ScheduleEventCommand : ICommand<Result>
{
    [Required]
    public string EventName { get; init; } = string.Empty;
    
    [FutureDate]
    public DateTime EventDate { get; init; }
    
    [PositiveNumber]
    public int MaxAttendees { get; init; }
}
```

## 📋 Error Handling

The extension throws `DataAnnotationsValidationException` with detailed error information:

```csharp
public class ValidationError
{
    public string PropertyName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

public class DataAnnotationsValidationException : Exception
{
    public IReadOnlyList<ValidationError> Errors { get; }
    
    public DataAnnotationsValidationException(IEnumerable<ValidationError> errors)
        : base("One or more validation failures occurred.")
    {
        Errors = errors.ToList();
    }
}
```

### Global Error Handling

```csharp
// In Program.cs or Startup.cs
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        
        if (exception is DataAnnotationsValidationException validationEx)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                Message = "Validation failed",
                Errors = validationEx.Errors.ToDictionary(
                    e => e.PropertyName,
                    e => e.ErrorMessage
                )
            };
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    });
});
```

## ⚡ Performance Tips

1. **Attribute Caching**: Validation attributes are cached by reflection
2. **Simple Validation**: DataAnnotations is optimized for simple field validation
3. **Avoid Complex Logic**: Use FluentValidation for complex business rules
4. **Property-Level Validation**: Focus on individual property validation rather than cross-property rules

## 🔧 Configuration Options

### Service Registration Options

```csharp
// Default registration
services.AddHandlrDataAnnotations();

// Manual registration for more control
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(DataAnnotationsBehavior<,>));
```

### Validation Context Options

```csharp
public class EnhancedDataAnnotationsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IServiceProvider _serviceProvider;
    
    public EnhancedDataAnnotationsBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // Enhanced validation context with service provider
        var validationContext = new ValidationContext(request, _serviceProvider, items: null);
        var validationResults = new List<ValidationResult>();
        
        bool isValid = Validator.TryValidateObject(
            request, 
            validationContext, 
            validationResults, 
            validateAllProperties: true
        );
        
        if (!isValid)
        {
            var errors = validationResults
                .SelectMany(vr => vr.MemberNames.Select(memberName => new ValidationError
                {
                    PropertyName = memberName,
                    ErrorMessage = vr.ErrorMessage ?? "Validation failed"
                }))
                .ToList();
                
            throw new DataAnnotationsValidationException(errors);
        }
        
        return await next();
    }
}
```

## 🔍 Troubleshooting

### Common Issues

1. **Validation Not Running**: Ensure `AddHandlrDataAnnotations()` is called
2. **Nested Object Validation**: Use `[ValidateComplexType]` for complex properties
3. **Collection Validation**: DataAnnotations has limited collection validation support

### Debug Logging

Enable debug logging to see validation behavior:

```json
{
  "Logging": {
    "LogLevel": {
      "Handlr.Extensions.DataAnnotations": "Debug"
    }
  }
}
```

## 🆚 DataAnnotations vs FluentValidation

| Feature | DataAnnotations | FluentValidation |
|---------|----------------|------------------|
| **Complexity** | Simple field validation | Complex business rules |
| **Dependencies** | Built into .NET | External library |
| **Performance** | Lightweight | More features, slightly heavier |
| **Async Validation** | Limited | Full support |
| **Cross-field Validation** | Limited | Excellent |
| **Custom Rules** | Attribute-based | Fluent API |
| **Error Messages** | Basic | Rich, conditional |

### When to Use DataAnnotations

- ✅ Simple field validation (required, length, range)
- ✅ Standard patterns (email, phone, URL)
- ✅ Minimal dependencies
- ✅ Quick prototyping
- ✅ Legacy .NET Framework compatibility

### When to Use FluentValidation

- ✅ Complex business rules
- ✅ Cross-field validation
- ✅ Async validation (database checks)
- ✅ Conditional validation
- ✅ Rich error messages

## 🔗 Related Packages

- **[Handlr.Abstractions](https://www.nuget.org/packages/Handlr.Abstractions/)**: Core CQRS interfaces
- **[Handlr.SourceGenerator](https://www.nuget.org/packages/Handlr.SourceGenerator/)**: Automatic code generation
- **[Handlr.Extensions.FluentValidation](https://www.nuget.org/packages/Handlr.Extensions.FluentValidation/)**: FluentValidation support

## 📖 Documentation

- [Main Handlr Documentation](https://github.com/sodiqyekeen/handlr)
- [Data Annotations Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations)
- [Pipeline Behaviors Guide](https://github.com/sodiqyekeen/handlr/blob/main/README-BEHAVIORS.md)

## 🤝 Contributing

Contributions are welcome! Please see the [main repository](https://github.com/sodiqyekeen/handlr) for contribution guidelines.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/sodiqyekeen/handlr/blob/main/LICENSE) file for details.

---

Built with ❤️ by [Sodiq Yekeen](https://github.com/sodiqyekeen) and the .NET community.
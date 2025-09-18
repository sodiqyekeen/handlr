using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentValidation;
using Handlr.Abstractions.Pipelines;
using Microsoft.Extensions.DependencyInjection;

namespace Handlr.Extensions.FluentValidation;

/// <summary>
/// Extension methods for adding FluentValidation support to Handlr CQRS framework.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds FluentValidation support to the Handlr CQRS framework.
    /// This registers the FluentValidation pipeline behavior and optionally discovers validators from assemblies.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Optional configuration for FluentValidation setup</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddHandlrFluentValidation(
        this IServiceCollection services,
        Action<FluentValidationOptions>? configureOptions = null)
    {
        var options = new FluentValidationOptions();
        configureOptions?.Invoke(options);

        // Register the FluentValidation pipeline behavior
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(FluentValidationBehavior<,>));

        // Auto-discover validators from specified assemblies
        foreach (var assembly in options.AssembliesToScan)
        {
            RegisterValidatorsFromAssembly(services, assembly);
        }

        return services;
    }

    /// <summary>
    /// Adds FluentValidation support and automatically discovers validators from the calling assembly.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddHandlrFluentValidation(this IServiceCollection services)
    {
        return services.AddHandlrFluentValidation(options =>
        {
            options.AssembliesToScan.Add(Assembly.GetCallingAssembly());
        });
    }

    /// <summary>
    /// Adds FluentValidation support and discovers validators from the specified assembly.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assembly">The assembly to scan for validators</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddHandlrFluentValidation(this IServiceCollection services, Assembly assembly)
    {
        return services.AddHandlrFluentValidation(options =>
        {
            options.AssembliesToScan.Add(assembly);
        });
    }

    /// <summary>
    /// Registers all FluentValidation validators from the specified assembly.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assembly">The assembly to scan for validators</param>
    private static void RegisterValidatorsFromAssembly(IServiceCollection services, Assembly assembly)
    {
        var validatorTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IValidator<>)))
            .ToList();

        foreach (var validatorType in validatorTypes)
        {
            var validatorInterfaces = validatorType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>))
                .ToList();

            foreach (var validatorInterface in validatorInterfaces)
            {
                services.AddScoped(validatorInterface, validatorType);
            }
        }
    }
}

/// <summary>
/// Configuration options for FluentValidation setup.
/// </summary>
public class FluentValidationOptions
{
    /// <summary>
    /// Gets the assemblies to scan for FluentValidation validators.
    /// </summary>
    public List<Assembly> AssembliesToScan { get; } = new();

    /// <summary>
    /// Gets or sets whether to include detailed validation errors in exceptions.
    /// </summary>
    public bool IncludeErrorDetails { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to validate all properties even if one fails.
    /// </summary>
    public bool ValidateAllProperties { get; set; } = true;
}

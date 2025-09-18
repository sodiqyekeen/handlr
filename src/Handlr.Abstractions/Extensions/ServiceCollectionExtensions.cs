using System;
using System.Reflection;
using Handlr.Abstractions.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Handlr.Abstractions.Extensions;

/// <summary>
/// Extension methods for registering Handlr services.
/// </summary>
public static class HandlrServiceCollectionExtensions
{
    /// <summary>
    /// Adds Handlr services and automatically registers all discovered handlers.
    /// When the source generator is present, it will automatically register optimized handlers.
    /// Without the source generator, only core services are registered and handlers must be added manually.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Optional configuration action</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddHandlr(this IServiceCollection services, Action<HandlrOptions>? configureOptions = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        var options = new HandlrOptions();
        configureOptions?.Invoke(options);

        // Register core services
        services.AddScoped<IHandlrDispatcher, HandlrDispatcher>();

        // Try to find and call the generated registration method
        TryCallGeneratedRegistration(services);

        return services;
    }

    private static void TryCallGeneratedRegistration(IServiceCollection services)
    {
        try
        {
            // Look for the generated extension method
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                // Look for generated extension class
                var generatedType = assembly.GetType("Handlr.Generated.Extensions.GeneratedHandlrServiceCollectionExtensions");
                if (generatedType != null)
                {
                    Console.WriteLine("� Found generated extensions class!");

                    // Find the TryAddGeneratedHandlers method
                    var method = generatedType.GetMethod("TryAddGeneratedHandlers",
                        BindingFlags.Public | BindingFlags.Static);

                    if (method != null)
                    {
                        method.Invoke(null, new object[] { services });
                        return;
                    }
                }
            }

            Console.WriteLine("⚠️ Generated handlers not found - using basic dispatcher");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error calling generated registration: {ex.Message}");
        }
    }

    /// <summary>
    /// Adds a command handler to the service collection manually.
    /// </summary>
    /// <typeparam name="TCommand">The command type</typeparam>
    /// <typeparam name="THandler">The handler type</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCommandHandler<TCommand, THandler>(this IServiceCollection services)
        where TCommand : class
        where THandler : class
    {
        services.AddScoped<THandler>();
        return services;
    }

    /// <summary>
    /// Adds a command handler with result to the service collection manually.
    /// </summary>
    /// <typeparam name="TCommand">The command type</typeparam>
    /// <typeparam name="TResult">The result type</typeparam>
    /// <typeparam name="THandler">The handler type</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCommandHandler<TCommand, TResult, THandler>(this IServiceCollection services)
        where TCommand : class
        where THandler : class
    {
        services.AddScoped<THandler>();
        return services;
    }

    /// <summary>
    /// Adds a query handler to the service collection manually.
    /// </summary>
    /// <typeparam name="TQuery">The query type</typeparam>
    /// <typeparam name="TResult">The result type</typeparam>
    /// <typeparam name="THandler">The handler type</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddQueryHandler<TQuery, TResult, THandler>(this IServiceCollection services)
        where TQuery : class
        where THandler : class
    {
        services.AddScoped<THandler>();
        return services;
    }
}

/// <summary>
/// Configuration options for Handlr.
/// </summary>
public class HandlrOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether debug information should be included.
    /// </summary>
    public bool IncludeDebugInfo { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether caching is enabled for queries.
    /// </summary>
    public bool EnableCaching { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether performance metrics should be collected.
    /// </summary>
    public bool EnableMetrics { get; set; } = false;

    /// <summary>
    /// Gets or sets the default timeout for operations.
    /// </summary>
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromMinutes(5);
}

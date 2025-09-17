using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Handlr.Abstractions.Common;

namespace Handlr.Abstractions.Extensions;

/// <summary>
/// Extension methods for registering Handlr services.
/// </summary>
public static class HandlrServiceCollectionExtensions
{
    /// <summary>
    /// Adds basic Handlr services to the service collection.
    /// If the source generator is installed, this will also automatically register discovered handlers.
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

        // Try to register discovered handlers if source generator is available
        TryRegisterDiscoveredHandlers(services);

        return services;
    }

    /// <summary>
    /// Attempts to register handlers discovered by the source generator.
    /// This is called automatically by AddHandlr() if the source generator is present.
    /// </summary>
    /// <param name="services">The service collection</param>
    private static void TryRegisterDiscoveredHandlers(IServiceCollection services)
    {
        try
        {
            // Look for the generated extension method in any loaded assembly
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var generatedExtensionsType = assembly.GetType("Handlr.Abstractions.Extensions.GeneratedHandlrServiceCollectionExtensions");
                if (generatedExtensionsType != null)
                {
                    var method = generatedExtensionsType.GetMethod("AddDiscoveredHandlrHandlers",
                        BindingFlags.Public | BindingFlags.Static);

                    if (method != null)
                    {
                        method.Invoke(null, new object[] { services });
                        break;
                    }
                }
            }
        }
        catch
        {
            // If source generator isn't available or discovery fails, continue without error
            // Users can manually register handlers if needed
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
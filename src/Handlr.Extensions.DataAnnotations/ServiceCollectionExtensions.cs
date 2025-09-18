using Handlr.Abstractions.Pipelines;
using Microsoft.Extensions.DependencyInjection;

namespace Handlr.Extensions.DataAnnotations;

/// <summary>
/// Extension methods for adding Data Annotations validation support to Handlr CQRS framework.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Data Annotations validation support to the Handlr CQRS framework.
    /// This registers the Data Annotations pipeline behavior that validates requests using System.ComponentModel.DataAnnotations attributes.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddHandlrDataAnnotations(this IServiceCollection services)
    {
        // Register the Data Annotations pipeline behavior
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(DataAnnotationsBehavior<,>));

        return services;
    }
}

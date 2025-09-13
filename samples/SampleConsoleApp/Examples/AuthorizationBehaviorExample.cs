using Handlr.Abstractions.Pipelines;

namespace SampleConsoleApp.Examples;

/// <summary>
/// Example interface that requests can implement to require authorization
/// </summary>
public interface IRequireAuthorization
{
    /// <summary>
    /// The required permission/role to execute this request
    /// </summary>
    string RequiredPermission { get; }

    /// <summary>
    /// Optional resource identifier for resource-based authorization
    /// </summary>
    string? ResourceId { get; }
}

/// <summary>
/// Example authorization behavior that checks permissions before processing requests
/// This shows how to implement cross-cutting authorization concerns
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class AuthorizationBehaviorExample<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequireAuthorization
{
    // In a real implementation, you would inject your authorization service
    // private readonly IAuthorizationService _authorizationService;
    // private readonly ICurrentUser _currentUser;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[AUTHORIZATION] Checking authorization for {typeof(TRequest).Name}");
        Console.WriteLine($"[AUTHORIZATION] Required permission: {request.RequiredPermission}");

        if (!string.IsNullOrEmpty(request.ResourceId))
        {
            Console.WriteLine($"[AUTHORIZATION] Resource-based authorization for resource: {request.ResourceId}");
        }

        // Example authorization logic
        // In a real implementation, you would:
        // 1. Get the current user from context
        // 2. Check if they have the required permission
        // 3. For resource-based auth, check access to specific resource

        var isAuthorized = await CheckAuthorizationAsync(request, cancellationToken);

        if (!isAuthorized)
        {
            Console.WriteLine($"[AUTHORIZATION] Access denied for {typeof(TRequest).Name}");
            throw new UnauthorizedAccessException($"Access denied. Required permission: {request.RequiredPermission}");
        }

        Console.WriteLine($"[AUTHORIZATION] Authorization successful for {typeof(TRequest).Name}");

        // Continue with the pipeline
        return await next();
    }

    private async Task<bool> CheckAuthorizationAsync(TRequest request, CancellationToken cancellationToken)
    {
        // Simulate authorization check
        await Task.Delay(10, cancellationToken);

        // Example logic - in reality, this would check against your authorization system
        // For demo purposes, we'll allow all requests except those requiring "Admin" permission
        return request.RequiredPermission != "Admin";
    }
}

/// <summary>
/// Example command that requires authorization
/// </summary>
public class SecureCommand : IRequireAuthorization
{
    public string Data { get; set; } = string.Empty;
    public string RequiredPermission => "User.Write";
    public string? ResourceId { get; set; }
}

/// <summary>
/// Example admin command that requires elevated permissions
/// </summary>
public class AdminCommand : IRequireAuthorization
{
    public string AdminAction { get; set; } = string.Empty;
    public string RequiredPermission => "Admin";
    public string? ResourceId => null; // System-level operation
}
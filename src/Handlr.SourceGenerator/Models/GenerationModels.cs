using Microsoft.CodeAnalysis;

namespace Handlr.SourceGenerator.Models;

/// <summary>
/// Represents a command discovered during source generation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the CommandInfo class.
/// </remarks>
/// <param name="typeSymbol">The command type symbol</param>
/// <param name="resultType">The result type if any</param>
/// <param name="location">The source location</param>
public class CommandInfo(INamedTypeSymbol typeSymbol, ITypeSymbol? resultType, Location location)
{
    /// <summary>
    /// Gets the command type symbol.
    /// </summary>
    public INamedTypeSymbol TypeSymbol { get; } = typeSymbol;

    /// <summary>
    /// Gets the full name of the command type.
    /// </summary>
    public string FullName { get; } = typeSymbol.ToDisplayString();

    /// <summary>
    /// Gets the namespace of the command type.
    /// </summary>
    public string Namespace { get; } = typeSymbol.ContainingNamespace.ToDisplayString();

    /// <summary>
    /// Gets the simple name of the command type.
    /// </summary>
    public string Name { get; } = typeSymbol.Name;

    /// <summary>
    /// Gets the result type if this is ICommand&lt;TResult&gt;, otherwise null.
    /// </summary>
    public ITypeSymbol? ResultType { get; } = resultType;

    /// <summary>
    /// Gets a value indicating whether this command has a result type.
    /// </summary>
    public bool HasResult => ResultType != null;

    /// <summary>
    /// Gets the handler interface name for this command.
    /// </summary>
    public string HandlerInterfaceName => HasResult
        ? $"ICommandHandler<{Name}, {ResultType?.ToDisplayString()}>"
        : $"ICommandHandler<{Name}>";

    /// <summary>
    /// Gets the generated handler class name.
    /// </summary>
    public string GeneratedHandlerName => $"{Name}Handler";

    /// <summary>
    /// Gets the handler method return type.
    /// </summary>
    public string HandlerReturnType => HasResult ? $"Task<{ResultType?.ToDisplayString()}>" : "Task";

    /// <summary>
    /// Gets the source location where this command was found.
    /// </summary>
    public Location Location { get; } = location;
}

/// <summary>
/// Represents a query discovered during source generation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the QueryInfo class.
/// </remarks>
/// <param name="typeSymbol">The query type symbol</param>
/// <param name="resultType">The result type</param>
/// <param name="location">The source location</param>
public class QueryInfo(INamedTypeSymbol typeSymbol, ITypeSymbol resultType, Location location)
{
    /// <summary>
    /// Gets the query type symbol.
    /// </summary>
    public INamedTypeSymbol TypeSymbol { get; } = typeSymbol;

    /// <summary>
    /// Gets the full name of the query type.
    /// </summary>
    public string FullName { get; } = typeSymbol.ToDisplayString();

    /// <summary>
    /// Gets the namespace of the query type.
    /// </summary>
    public string Namespace { get; } = typeSymbol.ContainingNamespace.ToDisplayString();

    /// <summary>
    /// Gets the simple name of the query type.
    /// </summary>
    public string Name { get; } = typeSymbol.Name;

    /// <summary>
    /// Gets the result type for this query.
    /// </summary>
    public ITypeSymbol ResultType { get; } = resultType;

    /// <summary>
    /// Gets the handler interface name for this query.
    /// </summary>
    public string HandlerInterfaceName => $"IQueryHandler<{Name}, {ResultType.ToDisplayString()}>";

    /// <summary>
    /// Gets the generated handler class name.
    /// </summary>
    public string GeneratedHandlerName => $"{Name}Handler";

    /// <summary>
    /// Gets the handler method return type.
    /// </summary>
    public string HandlerReturnType => $"Task<{ResultType.ToDisplayString()}>";

    /// <summary>
    /// Gets the source location where this query was found.
    /// </summary>
    public Location Location { get; } = location;
}

/// <summary>
/// Represents a pipeline behavior discovered during source generation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the PipelineBehaviorInfo class.
/// </remarks>
/// <param name="typeSymbol">The behavior type symbol</param>
/// <param name="requestType">The request type</param>
/// <param name="resultType">The result type</param>
/// <param name="isConditional">Whether this is a conditional behavior</param>
/// <param name="location">The source location</param>
public class PipelineBehaviorInfo(INamedTypeSymbol typeSymbol, ITypeSymbol requestType, ITypeSymbol resultType, bool isConditional, Location location)
{
    /// <summary>
    /// Gets the behavior type symbol.
    /// </summary>
    public INamedTypeSymbol TypeSymbol { get; } = typeSymbol;

    /// <summary>
    /// Gets the full name of the behavior type.
    /// </summary>
    public string FullName { get; } = typeSymbol.ToDisplayString();

    /// <summary>
    /// Gets the namespace of the behavior type.
    /// </summary>
    public string Namespace { get; } = typeSymbol.ContainingNamespace.ToDisplayString();

    /// <summary>
    /// Gets the simple name of the behavior type.
    /// </summary>
    public string Name { get; } = typeSymbol.Name;

    /// <summary>
    /// Gets the request type this behavior handles.
    /// </summary>
    public ITypeSymbol RequestType { get; } = requestType;

    /// <summary>
    /// Gets the result type this behavior handles.
    /// </summary>
    public ITypeSymbol ResultType { get; } = resultType;

    /// <summary>
    /// Gets a value indicating whether this is a conditional behavior.
    /// </summary>
    public bool IsConditional { get; } = isConditional;

    /// <summary>
    /// Gets the priority/order of this behavior (lower numbers execute first).
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// Gets the source location where this behavior was found.
    /// </summary>
    public Location Location { get; } = location;
}

/// <summary>
/// Represents a custom handler implementation discovered during source generation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the HandlerInfo class.
/// </remarks>
/// <param name="typeSymbol">The handler type symbol</param>
/// <param name="requestType">The request type</param>
/// <param name="resultType">The result type</param>
/// <param name="isCommandHandler">Whether this is a command handler</param>
/// <param name="location">The source location</param>
public class HandlerInfo(INamedTypeSymbol typeSymbol, ITypeSymbol requestType, ITypeSymbol? resultType, bool isCommandHandler, Location location)
{
    /// <summary>
    /// Gets the handler type symbol.
    /// </summary>
    public INamedTypeSymbol TypeSymbol { get; } = typeSymbol;

    /// <summary>
    /// Gets the full name of the handler type.
    /// </summary>
    public string FullName { get; } = typeSymbol.ToDisplayString();

    /// <summary>
    /// Gets the namespace of the handler type.
    /// </summary>
    public string Namespace { get; } = typeSymbol.ContainingNamespace.ToDisplayString();

    /// <summary>
    /// Gets the simple name of the handler type.
    /// </summary>
    public string Name { get; } = typeSymbol.Name;

    /// <summary>
    /// Gets the request type this handler handles.
    /// </summary>
    public ITypeSymbol RequestType { get; } = requestType;

    /// <summary>
    /// Gets the result type this handler returns.
    /// </summary>
    public ITypeSymbol? ResultType { get; } = resultType;

    /// <summary>
    /// Gets a value indicating whether this handler has a result type.
    /// </summary>
    public bool HasResult => ResultType != null;

    /// <summary>
    /// Gets a value indicating whether this is a command handler.
    /// </summary>
    public bool IsCommandHandler { get; } = isCommandHandler;

    /// <summary>
    /// Gets a value indicating whether this is a query handler.
    /// </summary>
    public bool IsQueryHandler => !IsCommandHandler;

    /// <summary>
    /// Gets the source location where this handler was found.
    /// </summary>
    public Location Location { get; } = location;
}

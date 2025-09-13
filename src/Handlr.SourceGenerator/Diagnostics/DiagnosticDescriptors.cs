using Microsoft.CodeAnalysis;

namespace Handlr.SourceGenerator.Diagnostics;

/// <summary>
/// Diagnostic descriptors for the Handlr source generator.
/// </summary>
public static class DiagnosticDescriptors
{
    private const string Category = "Handlr";

    /// <summary>
    /// Command class must have parameterless constructor.
    /// </summary>
    public static readonly DiagnosticDescriptor CommandMustHaveParameterlessConstructor = new(
        "HANDLR001",
        "Command class must have a parameterless constructor",
        "Command class '{0}' must have a parameterless constructor for source generation",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Command classes must have a parameterless constructor to be instantiated by the generated handler.");

    /// <summary>
    /// Query class must have parameterless constructor.
    /// </summary>
    public static readonly DiagnosticDescriptor QueryMustHaveParameterlessConstructor = new(
        "HANDLR002",
        "Query class must have a parameterless constructor",
        "Query class '{0}' must have a parameterless constructor for source generation",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Query classes must have a parameterless constructor to be instantiated by the generated handler.");

    /// <summary>
    /// Handler implementation not found.
    /// </summary>
    public static readonly DiagnosticDescriptor HandlerImplementationNotFound = new(
        "HANDLR003",
        "Handler implementation not found",
        "No handler implementation found for '{0}'",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A handler implementation should be provided for each command or query.");

    /// <summary>
    /// Pipeline behavior must implement IPipelineBehavior.
    /// </summary>
    public static readonly DiagnosticDescriptor PipelineBehaviorMustImplementInterface = new(
        "HANDLR004",
        "Pipeline behavior must implement IPipelineBehavior",
        "Pipeline behavior '{0}' must implement IPipelineBehavior<TRequest, TResult>",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Pipeline behaviors must implement the IPipelineBehavior interface.");

    /// <summary>
    /// Duplicate handler found.
    /// </summary>
    public static readonly DiagnosticDescriptor DuplicateHandlerFound = new(
        "HANDLR005",
        "Duplicate handler found",
        "Multiple handlers found for '{0}'",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Each command or query should have exactly one handler implementation.");

    /// <summary>
    /// Invalid return type for command.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidCommandReturnType = new(
        "HANDLR006",
        "Invalid return type for command",
        "Command '{0}' implements ICommand<{1}> but handler returns incompatible type '{2}'",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Command handler return type must match the command's result type.");

    /// <summary>
    /// Handler method signature is invalid.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidHandlerMethodSignature = new(
        "HANDLR007",
        "Invalid handler method signature",
        "Handler method '{0}' has invalid signature",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Handler methods must follow the expected signature pattern.");

    /// <summary>
    /// Pipeline behavior registration conflict.
    /// </summary>
    public static readonly DiagnosticDescriptor PipelineBehaviorRegistrationConflict = new(
        "HANDLR008",
        "Pipeline behavior registration conflict",
        "Pipeline behavior '{0}' is registered multiple times with different configurations",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Pipeline behaviors should be registered once with consistent configuration.");

    /// <summary>
    /// Missing pipeline behavior dependency.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingPipelineBehaviorDependency = new(
        "HANDLR009",
        "Missing pipeline behavior dependency",
        "Pipeline behavior '{0}' depends on service '{1}' which is not registered",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Pipeline behaviors may require additional service registrations.");

    /// <summary>
    /// Generated code formatting issue.
    /// </summary>
    public static readonly DiagnosticDescriptor GeneratedCodeFormattingIssue = new(
        "HANDLR010",
        "Generated code formatting issue",
        "Generated code for '{0}' may have formatting issues: {1}",
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Generated code formatting can be improved for better readability.");
}
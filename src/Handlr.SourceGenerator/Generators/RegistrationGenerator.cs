using System.Collections.Generic;
using Handlr.SourceGenerator.Models;
using Handlr.SourceGenerator.Templates;
using Microsoft.CodeAnalysis;

namespace Handlr.SourceGenerator.Generators;

/// <summary>
/// Generator for dependency injection registration code.
/// </summary>
public class RegistrationGenerator
{
    /// <summary>
    /// Generates service registration extensions for dependency injection.
    /// </summary>
    /// <param name="context">The source production context</param>
    /// <param name="commands">List of discovered commands</param>
    /// <param name="queries">List of discovered queries</param>
    /// <param name="behaviors">List of discovered pipeline behaviors</param>
    /// <param name="customHandlers">List of discovered custom handlers</param>
    /// <param name="includeDebugInfo">Whether to include debug information</param>
    public static void Generate(
        SourceProductionContext context,
        IEnumerable<CommandInfo> commands,
        IEnumerable<QueryInfo> queries,
        IEnumerable<PipelineBehaviorInfo> behaviors,
        IEnumerable<HandlerInfo> customHandlers,
        bool includeDebugInfo = false)
    {
        try
        {
            // Generate the registration code
            var registrationCode = RegistrationTemplate.Generate(
                commands,
                queries,
                behaviors,
                customHandlers,
                includeDebugInfo);

            // Add the generated source
            context.AddSource("HandlrServiceRegistration.g.cs", registrationCode);

            // Generate additional utility classes
            GenerateHandlerDispatcher(context, includeDebugInfo);
        }
        catch (System.Exception ex)
        {
            // Report generation error
            var diagnostic = Diagnostic.Create(
                Diagnostics.DiagnosticDescriptors.GeneratedCodeFormattingIssue,
                Location.None,
                "RegistrationGenerator",
                $"Error generating registration code: {ex.Message}");

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void GenerateHandlerDispatcher(
        SourceProductionContext context,
        bool includeDebugInfo)
    {
        // Use the template to generate the dispatcher
        var dispatcherCode = DispatcherTemplate.Generate(includeDebugInfo);
        context.AddSource("HandlrDispatcher.g.cs", dispatcherCode);
    }
}

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Handlr.SourceGenerator.Models;
using Handlr.SourceGenerator.Templates;
using Handlr.SourceGenerator.Diagnostics;

namespace Handlr.SourceGenerator.Generators;

/// <summary>
/// Generator for command and query handlers.
/// </summary>
public class HandlerGenerator
{
    /// <summary>
    /// Generates handler classes for the given commands and queries.
    /// </summary>
    /// <param name="context">The source production context</param>
    /// <param name="commands">List of discovered commands</param>
    /// <param name="queries">List of discovered queries</param>
    /// <param name="includeDebugInfo">Whether to include debug information</param>
    public static void Generate(
        SourceProductionContext context,
        IEnumerable<CommandInfo> commands,
        IEnumerable<QueryInfo> queries,
        bool includeDebugInfo = false)
    {
        // Generate command handlers
        foreach (var command in commands)
        {
            GenerateCommandHandler(context, command, includeDebugInfo);
        }

        // Generate query handlers
        foreach (var query in queries)
        {
            GenerateQueryHandler(context, query, includeDebugInfo);
        }
    }

    private static void GenerateCommandHandler(
        SourceProductionContext context,
        CommandInfo command,
        bool includeDebugInfo)
    {
        try
        {
            // Validate command
            if (!ValidateCommand(context, command))
                return;

            // Generate the handler code
            var handlerCode = CommandHandlerTemplate.Generate(command, includeDebugInfo);
            var fileName = $"{command.GeneratedHandlerName}.g.cs";

            // Add the generated source
            context.AddSource(fileName, handlerCode);

            if (includeDebugInfo)
            {
                // Add debug information
                var debugInfo = CreateDiagnostic(
                    DiagnosticDescriptors.GeneratedCodeFormattingIssue,
                    command.Location,
                    command.Name,
                    $"Generated handler: {fileName}");

                context.ReportDiagnostic(debugInfo);
            }
        }
        catch (System.Exception ex)
        {
            // Report generation error
            var diagnostic = CreateDiagnostic(
                DiagnosticDescriptors.GeneratedCodeFormattingIssue,
                command.Location,
                command.Name,
                $"Error generating handler: {ex.Message}");

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void GenerateQueryHandler(
        SourceProductionContext context,
        QueryInfo query,
        bool includeDebugInfo)
    {
        try
        {
            // Validate query
            if (!ValidateQuery(context, query))
                return;

            // Generate the handler code
            var handlerCode = QueryHandlerTemplate.Generate(query, includeDebugInfo);
            var fileName = $"{query.GeneratedHandlerName}.g.cs";

            // Add the generated source
            context.AddSource(fileName, handlerCode);

            if (includeDebugInfo)
            {
                // Add debug information
                var debugInfo = CreateDiagnostic(
                    DiagnosticDescriptors.GeneratedCodeFormattingIssue,
                    query.Location,
                    query.Name,
                    $"Generated handler: {fileName}");

                context.ReportDiagnostic(debugInfo);
            }
        }
        catch (System.Exception ex)
        {
            // Report generation error
            var diagnostic = CreateDiagnostic(
                DiagnosticDescriptors.GeneratedCodeFormattingIssue,
                query.Location,
                query.Name,
                $"Error generating handler: {ex.Message}");

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool ValidateCommand(SourceProductionContext context, CommandInfo command)
    {
        // Check for parameterless constructor
        var hasParameterlessConstructor = command.TypeSymbol.Constructors
            .Any(c => c.Parameters.Length == 0 && c.DeclaredAccessibility == Accessibility.Public);

        if (!hasParameterlessConstructor)
        {
            var diagnostic = CreateDiagnostic(
                DiagnosticDescriptors.CommandMustHaveParameterlessConstructor,
                command.Location,
                command.Name);

            context.ReportDiagnostic(diagnostic);
            return false;
        }

        return true;
    }

    private static bool ValidateQuery(SourceProductionContext context, QueryInfo query)
    {
        // Check for parameterless constructor
        var hasParameterlessConstructor = query.TypeSymbol.Constructors
            .Any(c => c.Parameters.Length == 0 && c.DeclaredAccessibility == Accessibility.Public);

        if (!hasParameterlessConstructor)
        {
            var diagnostic = CreateDiagnostic(
                DiagnosticDescriptors.QueryMustHaveParameterlessConstructor,
                query.Location,
                query.Name);

            context.ReportDiagnostic(diagnostic);
            return false;
        }

        return true;
    }

    private static Diagnostic CreateDiagnostic(DiagnosticDescriptor descriptor, Location location, params object[] args)
    {
        return Diagnostic.Create(descriptor, location, args);
    }
}
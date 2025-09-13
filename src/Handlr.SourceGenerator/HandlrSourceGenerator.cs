using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Handlr.SourceGenerator.Diagnostics;
using Handlr.SourceGenerator.Generators;
using Handlr.SourceGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Handlr.SourceGenerator;

/// <summary>
/// Incremental source generator for Handlr CQRS framework.
/// Generates handlers, pipeline behaviors, and service registrations.
/// </summary>
[Generator]
public class HandlrSourceGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the incremental generator.
    /// </summary>
    /// <param name="context">The initialization context</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add debug support - enable with HANDLR_DEBUG compilation symbol
#if DEBUG || HANDLR_DEBUG
        System.Diagnostics.Debugger.Launch();
#endif

        // Create a syntax provider that finds CQRS elements
        var cqrsElements = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsCandidateForGeneration(s),
                transform: static (ctx, _) => GetCqrsElements(ctx))
            .Where(static elements => elements != null)
            .Select(static (elements, _) => elements!);

        // Combine with compilation and generate sources
        var compilationAndElements = context.CompilationProvider.Combine(cqrsElements.Collect());

        context.RegisterSourceOutput(compilationAndElements, static (ctx, source) => Execute(ctx, source.Left, source.Right));
    }

    private static bool IsCandidateForGeneration(SyntaxNode node)
    {
        // Look for class or record declarations that might implement CQRS interfaces
        if (node is ClassDeclarationSyntax classDeclaration)
        {
            // Check if class has any base types (interfaces or base classes)
            return classDeclaration.BaseList?.Types.Count > 0;
        }

        if (node is RecordDeclarationSyntax recordDeclaration)
        {
            // Check if record has any base types (interfaces or base classes)
            return recordDeclaration.BaseList?.Types.Count > 0;
        }

        return false;
    }

    private static CqrsElements? GetCqrsElements(GeneratorSyntaxContext context)
    {
        INamedTypeSymbol? classSymbol = null;
        Location? location = null;

        if (context.Node is ClassDeclarationSyntax classDeclaration)
        {
            var semanticModel = context.SemanticModel;
            classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
            location = classDeclaration.Identifier.GetLocation();
        }
        else if (context.Node is RecordDeclarationSyntax recordDeclaration)
        {
            var semanticModel = context.SemanticModel;
            classSymbol = semanticModel.GetDeclaredSymbol(recordDeclaration) as INamedTypeSymbol;
            location = recordDeclaration.Identifier.GetLocation();
        }

        if (classSymbol == null || classSymbol.IsAbstract || location == null)
            return null;

        // Debug: Generate a file for each class being analyzed
        var debugSource = $@"
// Debug info for class {classSymbol.Name}
// Interfaces: {string.Join(", ", classSymbol.AllInterfaces.Select(i => i.ToDisplayString()))}
// Full name: {classSymbol.ToDisplayString()}
";
        // Note: We can't add source here because we don't have SourceProductionContext

        var elements = new CqrsElements();
        var interfaces = classSymbol.AllInterfaces;

        // First, check if this is a command or query by looking for the most specific interface
        var commandInterfaces = interfaces.Where(i =>
            i.OriginalDefinition.ToDisplayString() == "Handlr.Abstractions.Commands.ICommand" ||
            i.OriginalDefinition.ToDisplayString() == "Handlr.Abstractions.Commands.ICommand<TResult>").ToList();

        var queryInterfaces = interfaces.Where(i =>
            i.OriginalDefinition.ToDisplayString() == "Handlr.Abstractions.Queries.IQuery<TResult>").ToList();

        // Process commands - prioritize generic interface over base interface
        if (commandInterfaces.Any())
        {
            var commandInterface = commandInterfaces
                .FirstOrDefault(i => i.OriginalDefinition.ToDisplayString() == "Handlr.Abstractions.Commands.ICommand<TResult>") ??
                commandInterfaces.First();

            ITypeSymbol? resultType = null;
            if (commandInterface.TypeArguments.Length == 1)
                resultType = commandInterface.TypeArguments[0];

            elements.Commands.Add(new CommandInfo(classSymbol, resultType, location));
        }
        // Process queries
        else if (queryInterfaces.Any())
        {
            var queryInterface = queryInterfaces.First();
            if (queryInterface.TypeArguments.Length == 1)
            {
                var resultType = queryInterface.TypeArguments[0];
                elements.Queries.Add(new QueryInfo(classSymbol, resultType, location));
            }
        }

        // Process other interface types
        foreach (var @interface in interfaces)
        {
            var interfaceFullName = @interface.OriginalDefinition.ToDisplayString();

            // Check for pipeline behaviors - use OriginalDefinition
            if (interfaceFullName == "Handlr.Abstractions.Pipelines.IPipelineBehavior<TRequest, TResult>" &&
                     @interface.TypeArguments.Length == 2)
            {
                var requestType = @interface.TypeArguments[0];
                var resultType = @interface.TypeArguments[1];

                var isConditional = interfaces.Any(i =>
                    i.OriginalDefinition.ToDisplayString() == "Handlr.Abstractions.Pipelines.IConditionalBehavior<TRequest, TResult>");

                elements.Behaviors.Add(new PipelineBehaviorInfo(classSymbol, requestType, resultType, isConditional, location));
            }
            // Check for custom handlers - use OriginalDefinition
            else if (interfaceFullName == "Handlr.Abstractions.Commands.ICommandHandler<TCommand>" ||
                     interfaceFullName == "Handlr.Abstractions.Commands.ICommandHandler<TCommand, TResult>" ||
                     interfaceFullName == "Handlr.Abstractions.Queries.IQueryHandler<TQuery, TResult>")
            {
                var isCommandHandler = interfaceFullName.Contains("Command");

                if (@interface.TypeArguments.Length == 1)
                {
                    var requestType = @interface.TypeArguments[0];
                    elements.CustomHandlers.Add(new HandlerInfo(classSymbol, requestType, null, isCommandHandler, location));
                }
                else if (@interface.TypeArguments.Length == 2)
                {
                    var requestType = @interface.TypeArguments[0];
                    var resultType = @interface.TypeArguments[1];
                    elements.CustomHandlers.Add(new HandlerInfo(classSymbol, requestType, resultType, isCommandHandler, location));
                }
            }
        }

        return elements.HasAnyElements ? elements : null;
    }

    private static void Execute(SourceProductionContext context, Compilation compilation, ImmutableArray<CqrsElements> allElements)
    {
        try
        {
            // Combine all discovered elements
            var combinedElements = CombineElements(allElements);

            // Generate a debug file to confirm source generator is running
            var debugClasses = allElements
                .SelectMany(e => e.Commands.Select(c => $"Command: {c.Name}"))
                .Concat(allElements.SelectMany(e => e.Queries.Select(q => $"Query: {q.Name}")))
                .Concat(allElements.SelectMany(e => e.Behaviors.Select(b => $"Behavior: {b.Name}")))
                .ToList();

            var debugSource = $@"
namespace Handlr.Generated
{{
    public static class DebugInfo
    {{
        public static string GeneratedAt => ""{DateTime.Now:yyyy-MM-dd HH:mm:ss}"";
        public static int CommandsFound => {combinedElements.Commands.Count()};
        public static int QueriesFound => {combinedElements.Queries.Count()};
        public static int BehaviorsFound => {combinedElements.Behaviors.Count()};
        public static int CustomHandlersFound => {combinedElements.CustomHandlers.Count()};
        public static int ElementGroups => {allElements.Length};
        public static string[] FoundClasses => new string[] {{ {string.Join(", ", debugClasses.Select(c => $"\"{c}\""))} }};
    }}
}}";
            context.AddSource("DebugInfo.g.cs", SourceText.From(debugSource, Encoding.UTF8));

            // Generate interface debug info to understand what interfaces are being checked
            var interfaceDebugSource = GenerateInterfaceDebugInfo(compilation);
            context.AddSource("InterfaceDebugInfo.g.cs", SourceText.From(interfaceDebugSource, Encoding.UTF8));

            // Determine if debug info should be included
            var includeDebugInfo = ShouldIncludeDebugInfo(compilation);

            // Validate for duplicate handlers
            ValidateForDuplicates(context, combinedElements);

            // Generate handlers
            HandlerGenerator.Generate(
                context,
                combinedElements.Commands,
                combinedElements.Queries,
                includeDebugInfo);

            // Generate pipeline configuration (disabled temporarily)
            // PipelineGenerator.Generate(
            //     context,
            //     combinedElements.Behaviors,
            //     includeDebugInfo);

            // Generate service registrations
            RegistrationGenerator.Generate(
                context,
                combinedElements.Commands,
                combinedElements.Queries,
                combinedElements.Behaviors,
                combinedElements.CustomHandlers,
                includeDebugInfo);

            // Report summary information if debug is enabled
            if (includeDebugInfo)
            {
                ReportGenerationSummary(context, combinedElements);
            }
        }
        catch (System.Exception ex)
        {
            // Report critical generation error
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.GeneratedCodeFormattingIssue,
                Location.None,
                "HandlrSourceGenerator",
                $"Critical error during generation: {ex.Message}");

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static string GenerateInterfaceDebugInfo(Compilation compilation)
    {
        var classInfo = new List<string>();

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var classes = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
            var records = syntaxTree.GetRoot().DescendantNodes().OfType<RecordDeclarationSyntax>();

            // Process classes
            foreach (var classDecl in classes)
            {
                var classSymbol = semanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
                if (classSymbol == null) continue;

                // Only include classes from our sample project
                if (!classSymbol.ContainingNamespace.ToDisplayString().StartsWith("SampleConsoleApp"))
                    continue;

                ProcessTypeForDebug(classSymbol, classInfo);
            }

            // Process records
            foreach (var recordDecl in records)
            {
                var recordSymbol = semanticModel.GetDeclaredSymbol(recordDecl) as INamedTypeSymbol;
                if (recordSymbol == null) continue;

                // Only include records from our sample project
                if (!recordSymbol.ContainingNamespace.ToDisplayString().StartsWith("SampleConsoleApp"))
                    continue;

                ProcessTypeForDebug(recordSymbol, classInfo);
            }
        }

        var classDebugInfo = string.Join("\", \"", classInfo);

        return $@"
namespace Handlr.Generated
{{
    public static class InterfaceDebugInfo
    {{
        public static string[] ClassInterfaces => new string[] {{ ""{classDebugInfo}"" }};
    }}
}}";
    }

    private static void ProcessTypeForDebug(INamedTypeSymbol typeSymbol, List<string> classInfo)
    {
        var interfaces = typeSymbol.AllInterfaces.Select(i => new
        {
            Name = i.ToDisplayString(),
            OriginalDefinition = i.OriginalDefinition.ToDisplayString(),
            TypeArguments = i.TypeArguments.Length
        }).ToArray();

        if (interfaces.Any())
        {
            var interfaceList = string.Join(", ", interfaces.Select(i => $"{i.OriginalDefinition}({i.TypeArguments})"));
            classInfo.Add($"{typeSymbol.Name}: {interfaceList}");
        }
        else if (typeSymbol.BaseType != null && typeSymbol.BaseType.Name != "Object")
        {
            classInfo.Add($"{typeSymbol.Name}: BaseType: {typeSymbol.BaseType.ToDisplayString()}");
        }
        else
        {
            classInfo.Add($"{typeSymbol.Name}: NO_INTERFACES_OR_BASE");
        }
    }

    private static CqrsElements CombineElements(ImmutableArray<CqrsElements> allElements)
    {
        var combined = new CqrsElements();

        foreach (var elements in allElements)
        {
            combined.Commands.AddRange(elements.Commands);
            combined.Queries.AddRange(elements.Queries);
            combined.Behaviors.AddRange(elements.Behaviors);
            combined.CustomHandlers.AddRange(elements.CustomHandlers);
        }

        return combined;
    }

    private static bool ShouldIncludeDebugInfo(Compilation compilation)
    {
        // Check for debug compilation symbols
        return compilation.SyntaxTrees.Any(tree =>
            tree.GetCompilationUnitRoot().GetFirstDirective(directive =>
                directive is DefineDirectiveTriviaSyntax define &&
                (define.Name.ValueText == "DEBUG" || define.Name.ValueText == "HANDLR_DEBUG")) != null);
    }

    private static void ValidateForDuplicates(SourceProductionContext context, CqrsElements elements)
    {
        // Check for duplicate command handlers
        var commandGroups = elements.Commands.GroupBy(c => c.FullName);
        foreach (var group in commandGroups.Where(g => g.Count() > 1))
        {
            foreach (var duplicate in group.Skip(1))
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.DuplicateHandlerFound,
                    duplicate.Location,
                    duplicate.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }

        // Check for duplicate query handlers
        var queryGroups = elements.Queries.GroupBy(q => q.FullName);
        foreach (var group in queryGroups.Where(g => g.Count() > 1))
        {
            foreach (var duplicate in group.Skip(1))
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.DuplicateHandlerFound,
                    duplicate.Location,
                    duplicate.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static void ReportGenerationSummary(SourceProductionContext context, CqrsElements elements)
    {
        var summary = $"Generated: {elements.Commands.Count} commands, {elements.Queries.Count} queries, " +
                     $"{elements.Behaviors.Count} behaviors, {elements.CustomHandlers.Count} custom handlers";

        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.GeneratedCodeFormattingIssue,
            Location.None,
            "HandlrSourceGenerator",
            summary);

        context.ReportDiagnostic(diagnostic);
    }

    /// <summary>
    /// Container for discovered CQRS elements.
    /// </summary>
    internal class CqrsElements
    {
        public System.Collections.Generic.List<CommandInfo> Commands { get; } = new();
        public System.Collections.Generic.List<QueryInfo> Queries { get; } = new();
        public System.Collections.Generic.List<PipelineBehaviorInfo> Behaviors { get; } = new();
        public System.Collections.Generic.List<HandlerInfo> CustomHandlers { get; } = new();

        public bool HasAnyElements => Commands.Any() || Queries.Any() || Behaviors.Any() || CustomHandlers.Any();
    }
}

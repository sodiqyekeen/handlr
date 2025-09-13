using System.Collections.Generic;
using System.Linq;
using Handlr.SourceGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Handlr.SourceGenerator.Core;

/// <summary>
/// Syntax context receiver that identifies CQRS elements for source generation.
/// </summary>
public class HandlrSyntaxContextReceiver : ISyntaxContextReceiver
{
    /// <summary>
    /// Gets the list of discovered commands.
    /// </summary>
    public List<CommandInfo> Commands { get; } = new();

    /// <summary>
    /// Gets the list of discovered queries.
    /// </summary>
    public List<QueryInfo> Queries { get; } = new();

    /// <summary>
    /// Gets the list of discovered pipeline behaviors.
    /// </summary>
    public List<PipelineBehaviorInfo> PipelineBehaviors { get; } = new();

    /// <summary>
    /// Gets the list of discovered custom handlers.
    /// </summary>
    public List<HandlerInfo> CustomHandlers { get; } = new();

    /// <summary>
    /// Called for every syntax node in the compilation.
    /// </summary>
    /// <param name="context">The generator syntax context</param>
    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        // Look for class declarations
        if (context.Node is ClassDeclarationSyntax classDeclaration)
        {
            var semanticModel = context.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

            if (classSymbol == null || classSymbol.IsAbstract)
                return;

            // Check for command implementations
            CheckForCommandImplementation(classSymbol, classDeclaration);

            // Check for query implementations
            CheckForQueryImplementation(classSymbol, classDeclaration);

            // Check for pipeline behavior implementations
            CheckForPipelineBehaviorImplementation(classSymbol, classDeclaration);

            // Check for custom handler implementations
            CheckForCustomHandlerImplementation(classSymbol, classDeclaration);
        }
    }

    private void CheckForCommandImplementation(INamedTypeSymbol classSymbol, ClassDeclarationSyntax classDeclaration)
    {
        var interfaces = classSymbol.AllInterfaces;

        foreach (var @interface in interfaces)
        {
            var interfaceName = @interface.ToDisplayString();

            // Check for ICommand
            if (interfaceName.StartsWith("Handlr.Abstractions.Commands.ICommand") &&
                !interfaceName.Contains("Handler"))
            {
                ITypeSymbol? resultType = null;

                // Check if it's ICommand<TResult>
                if (@interface.TypeArguments.Length == 1)
                {
                    resultType = @interface.TypeArguments[0];
                }

                var location = classDeclaration.Identifier.GetLocation();
                var commandInfo = new CommandInfo(classSymbol, resultType, location);
                Commands.Add(commandInfo);
                break; // Only process the first matching interface
            }
        }
    }

    private void CheckForQueryImplementation(INamedTypeSymbol classSymbol, ClassDeclarationSyntax classDeclaration)
    {
        var interfaces = classSymbol.AllInterfaces;

        foreach (var @interface in interfaces)
        {
            var interfaceName = @interface.ToDisplayString();

            // Check for IQuery<TResult>
            if (interfaceName.StartsWith("Handlr.Abstractions.Queries.IQuery") &&
                @interface.TypeArguments.Length == 1)
            {
                var resultType = @interface.TypeArguments[0];
                var location = classDeclaration.Identifier.GetLocation();
                var queryInfo = new QueryInfo(classSymbol, resultType, location);
                Queries.Add(queryInfo);
                break; // Only process the first matching interface
            }
        }
    }

    private void CheckForPipelineBehaviorImplementation(INamedTypeSymbol classSymbol, ClassDeclarationSyntax classDeclaration)
    {
        var interfaces = classSymbol.AllInterfaces;

        foreach (var @interface in interfaces)
        {
            var interfaceName = @interface.ToDisplayString();

            // Check for IPipelineBehavior<TRequest, TResult>
            if (interfaceName.StartsWith("Handlr.Abstractions.Pipelines.IPipelineBehavior") &&
                @interface.TypeArguments.Length == 2)
            {
                var requestType = @interface.TypeArguments[0];
                var resultType = @interface.TypeArguments[1];
                var location = classDeclaration.Identifier.GetLocation();

                // Check if it's also IConditionalBehavior
                var isConditional = interfaces.Any(i =>
                    i.ToDisplayString().StartsWith("Handlr.Abstractions.Pipelines.IConditionalBehavior"));

                var behaviorInfo = new PipelineBehaviorInfo(classSymbol, requestType, resultType, isConditional, location);
                PipelineBehaviors.Add(behaviorInfo);
                break; // Only process the first matching interface
            }
        }
    }

    private void CheckForCustomHandlerImplementation(INamedTypeSymbol classSymbol, ClassDeclarationSyntax classDeclaration)
    {
        var interfaces = classSymbol.AllInterfaces;

        foreach (var @interface in interfaces)
        {
            var interfaceName = @interface.ToDisplayString();

            // Check for ICommandHandler<TCommand> or ICommandHandler<TCommand, TResult>
            if (interfaceName.StartsWith("Handlr.Abstractions.Commands.ICommandHandler"))
            {
                var location = classDeclaration.Identifier.GetLocation();

                if (@interface.TypeArguments.Length == 1)
                {
                    // ICommandHandler<TCommand>
                    var requestType = @interface.TypeArguments[0];
                    var handlerInfo = new HandlerInfo(classSymbol, requestType, null, true, location);
                    CustomHandlers.Add(handlerInfo);
                }
                else if (@interface.TypeArguments.Length == 2)
                {
                    // ICommandHandler<TCommand, TResult>
                    var requestType = @interface.TypeArguments[0];
                    var resultType = @interface.TypeArguments[1];
                    var handlerInfo = new HandlerInfo(classSymbol, requestType, resultType, true, location);
                    CustomHandlers.Add(handlerInfo);
                }
                break;
            }

            // Check for IQueryHandler<TQuery, TResult>
            if (interfaceName.StartsWith("Handlr.Abstractions.Queries.IQueryHandler") &&
                @interface.TypeArguments.Length == 2)
            {
                var requestType = @interface.TypeArguments[0];
                var resultType = @interface.TypeArguments[1];
                var location = classDeclaration.Identifier.GetLocation();
                var handlerInfo = new HandlerInfo(classSymbol, requestType, resultType, false, location);
                CustomHandlers.Add(handlerInfo);
                break;
            }
        }
    }
}

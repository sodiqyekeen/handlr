using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Handlr.SourceGenerator;

[Generator]
public class HandlrSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new HandlrSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not HandlrSyntaxReceiver receiver)
            return;

        var dispatcherCode = GenerateDispatcherImplementation(receiver);
        context.AddSource("GeneratedHandlrDispatcher.g.cs", SourceText.From(dispatcherCode, Encoding.UTF8));

        var registrationCode = GenerateRegistrationImplementation(receiver);
        context.AddSource("GeneratedHandlrRegistration.g.cs", SourceText.From(registrationCode, Encoding.UTF8));
    }

    private static string GenerateDispatcherImplementation(HandlrSyntaxReceiver receiver)
    {
        var builder = new StringBuilder();
        builder.AppendLine("#nullable enable");
        builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        builder.AppendLine("using Handlr.Abstractions.Common;");
        builder.AppendLine("using Handlr.Abstractions.Commands;");
        builder.AppendLine("using Handlr.Abstractions.Queries;");
        builder.AppendLine("using Handlr.Abstractions.Pipelines;");
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine("using System.Linq;");
        builder.AppendLine("using System.Threading;");
        builder.AppendLine("using System.Threading.Tasks;");
        builder.AppendLine();
        builder.AppendLine("namespace Handlr.Generated;");
        builder.AppendLine();
        builder.AppendLine("/// <summary>");
        builder.AppendLine("/// High-performance generated dispatcher with zero reflection and full pipeline behavior support.");
        builder.AppendLine("/// This implementation combines Click CQRS patterns with Handlr's powerful pipeline behaviors.");
        builder.AppendLine("/// </summary>");
        builder.AppendLine("public class GeneratedHandlrDispatcher : IHandlrDispatcher");
        builder.AppendLine("{");
        builder.AppendLine("    private readonly IServiceProvider _serviceProvider;");
        builder.AppendLine();
        builder.AppendLine("    public GeneratedHandlrDispatcher(IServiceProvider serviceProvider)");
        builder.AppendLine("    {");
        builder.AppendLine("        _serviceProvider = serviceProvider;");
        builder.AppendLine("    }");

        // Generate pipeline-aware methods
        GeneratePipelineAwareSendCommandMethod(builder, receiver);
        GeneratePipelineAwareSendCommandWithResultMethod(builder, receiver);
        GeneratePipelineAwareSendQueryMethod(builder, receiver);

        // Generate pipeline execution helper
        GeneratePipelineExecutionHelper(builder);

        builder.AppendLine("}");
        return builder.ToString();
    }

    private static void GeneratePipelineAwareSendCommandMethod(StringBuilder builder, HandlrSyntaxReceiver receiver)
    {
        builder.AppendLine("    public async Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand");
        builder.AppendLine("    {");
        builder.AppendLine("        if (command == null) throw new ArgumentNullException(nameof(command));");
        builder.AppendLine();
        builder.AppendLine("        // Build and execute pipeline for commands without results");
        builder.AppendLine("        await ExecutePipeline<TCommand, object>(command, async () => {");

        if (receiver.Commands.Any())
        {
            builder.AppendLine("            await (command switch");
            builder.AppendLine("            {");

            foreach (var command in receiver.Commands)
            {
                var fullName = command.ToDisplayString();
                builder.AppendLine($"                {fullName} cmd => HandleCommand_{command.Name}(cmd, cancellationToken),");
            }

            builder.AppendLine("                _ => throw new InvalidOperationException($\"Command type {command.GetType().Name} is not supported.\")");
            builder.AppendLine("            });");
            builder.AppendLine("            return null!; // Commands without results return null");
        }
        else
        {
            builder.AppendLine("            await Task.FromException(new InvalidOperationException($\"Command type {command.GetType().Name} is not supported.\"));");
            builder.AppendLine("            return null!;");
        }

        builder.AppendLine("        }, cancellationToken);");
        builder.AppendLine("    }");

        // Generate individual command handlers
        foreach (var command in receiver.Commands)
        {
            var fullName = command.ToDisplayString();
            builder.AppendLine($@"    
    private async Task HandleCommand_{command.Name}({fullName} command, CancellationToken cancellationToken)
    {{
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<{fullName}>>();
        await handler.Handle(command, cancellationToken);
    }}");
        }
    }

    private static void GeneratePipelineAwareSendCommandWithResultMethod(StringBuilder builder, HandlrSyntaxReceiver receiver)
    {
        builder.AppendLine("    public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (command == null) throw new ArgumentNullException(nameof(command));");
        builder.AppendLine();
        builder.AppendLine("        // Build and execute pipeline for commands with results");
        builder.AppendLine("        return command switch");
        builder.AppendLine("        {");

        if (receiver.CommandsWithResults.Any())
        {
            foreach (var command in receiver.CommandsWithResults)
            {
                var fullName = command.ToDisplayString();
                var resultType = GetResultType(command);
                builder.AppendLine($"            {fullName} cmd => (TResult)(object)await ExecutePipeline<{fullName}, {resultType}>(cmd, async () => await HandleCommandWithResult_{command.Name}(cmd, cancellationToken), cancellationToken),");
            }

            builder.AppendLine("            _ => throw new InvalidOperationException($\"Command type {command.GetType().Name} is not supported.\")");
        }
        else
        {
            builder.AppendLine("            _ => throw new InvalidOperationException($\"Command type {command.GetType().Name} is not supported.\")");
        }

        builder.AppendLine("        };");
        builder.AppendLine("    }");

        // Generate individual command handlers with results
        foreach (var command in receiver.CommandsWithResults)
        {
            var fullName = command.ToDisplayString();
            var resultType = GetResultType(command);
            builder.AppendLine($@"    
    private async Task<{resultType}> HandleCommandWithResult_{command.Name}({fullName} command, CancellationToken cancellationToken)
    {{
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<{fullName}, {resultType}>>();
        return await handler.Handle(command, cancellationToken);
    }}");
        }
    }

    private static void GeneratePipelineAwareSendQueryMethod(StringBuilder builder, HandlrSyntaxReceiver receiver)
    {
        builder.AppendLine("    public async Task<TResult> SendAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (query == null) throw new ArgumentNullException(nameof(query));");
        builder.AppendLine();
        builder.AppendLine("        // Build and execute pipeline for queries");
        builder.AppendLine("        return await ExecutePipeline<IQuery<TResult>, TResult>(query, async () => {");

        if (receiver.Queries.Any())
        {
            builder.AppendLine("            return query switch");
            builder.AppendLine("            {");

            foreach (var query in receiver.Queries)
            {
                var fullName = query.ToDisplayString();
                var resultType = GetResultType(query);
                builder.AppendLine($"                {fullName} q => (TResult)(object)await HandleQuery_{query.Name}(q, cancellationToken),");
            }

            builder.AppendLine("                _ => throw new InvalidOperationException($\"Query type {query.GetType().Name} is not supported.\")");
            builder.AppendLine("            };");
        }
        else
        {
            builder.AppendLine("            return await Task.FromException<TResult>(new InvalidOperationException($\"Query type {query.GetType().Name} is not supported.\"));");
        }

        builder.AppendLine("        }, cancellationToken);");
        builder.AppendLine("    }");

        // Generate individual query handlers
        foreach (var query in receiver.Queries)
        {
            var fullName = query.ToDisplayString();
            var resultType = GetResultType(query);
            builder.AppendLine($@"    
    private async Task<{resultType}> HandleQuery_{query.Name}({fullName} query, CancellationToken cancellationToken)
    {{
        var handler = _serviceProvider.GetRequiredService<IQueryHandler<{fullName}, {resultType}>>();
        return await handler.Handle(query, cancellationToken);
    }}");
        }
    }

    private static void GeneratePipelineExecutionHelper(StringBuilder builder)
    {
        builder.AppendLine(@"
    /// <summary>
    /// Executes the pipeline behavior chain for the given request.
    /// This method builds the pipeline chain and executes behaviors in order.
    /// </summary>
    private async Task<TResult> ExecutePipeline<TRequest, TResult>(TRequest request, Func<Task<TResult>> handler, CancellationToken cancellationToken)
    {
        // Get all pipeline behaviors for this request/result type
        // The DI container should resolve open generics to closed generics
        var behaviors = _serviceProvider.GetServices<IPipelineBehavior<TRequest, TResult>>().ToArray();
        
        // Also try to get behaviors using reflection as fallback for open generic registrations
        if (behaviors.Length == 0)
        {
            var pipelineBehaviorType = typeof(IPipelineBehavior<,>);
            var closedGenericType = pipelineBehaviorType.MakeGenericType(typeof(TRequest), typeof(TResult));
            behaviors = ((IEnumerable<IPipelineBehavior<TRequest, TResult>>)_serviceProvider.GetServices(closedGenericType)).ToArray();
        }
        
        if (behaviors.Length == 0)
        {
            // No behaviors, execute handler directly
            return await handler();
        }
        
        // Build the pipeline chain from right to left (handler -> behavior N -> ... -> behavior 1)
        RequestHandlerDelegate<TResult> pipeline = async () => await handler();

        // Wrap each behavior around the pipeline
        for (int i = behaviors.Length - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var currentPipeline = pipeline;
            pipeline = async () => await behavior.Handle(request, currentPipeline, cancellationToken);
        }

        // Execute the complete pipeline
        return await pipeline();
    }");
    }

    private static string GenerateRegistrationImplementation(HandlrSyntaxReceiver receiver)
    {
        var builder = new StringBuilder();

        // Generate the using statements
        builder.AppendLine("#nullable enable");
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Linq;");
        builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        builder.AppendLine("using Handlr.Abstractions.Common;");
        builder.AppendLine("using Handlr.Abstractions.Commands;");
        builder.AppendLine("using Handlr.Abstractions.Queries;");
        builder.AppendLine("using Handlr.Abstractions.Extensions;");
        builder.AppendLine();

        // Generate the namespace and class  
        builder.AppendLine("namespace Handlr.Generated.Extensions;");
        builder.AppendLine();
        builder.AppendLine("/// <summary>");
        builder.AppendLine("/// Auto-generated extension methods that provide optimized handler registration.");
        builder.AppendLine("/// This extends the base AddHandlr method with source-generated handler discovery.");
        builder.AppendLine("/// </summary>");
        builder.AppendLine("public static class GeneratedHandlrServiceCollectionExtensions");
        builder.AppendLine("{");

        // Generate the TryAddGeneratedHandlers method
        builder.AppendLine("    /// <summary>");
        builder.AppendLine("    /// Registers all discovered handlers and replaces the basic dispatcher with the optimized one.");
        builder.AppendLine("    /// This method is called automatically by AddHandlr() when source generation is enabled.");
        builder.AppendLine("    /// </summary>");
        builder.AppendLine("    /// <param name=\"services\">The service collection</param>");
        builder.AppendLine("    /// <returns>The service collection for chaining</returns>");
        builder.AppendLine("    public static IServiceCollection TryAddGeneratedHandlers(this IServiceCollection services)");
        builder.AppendLine("    {");
        builder.AppendLine("        // Replace the basic dispatcher with our optimized generated one");
        builder.AppendLine("        // Remove any existing IHandlrDispatcher registration and add the generated one");
        builder.AppendLine("        var existingDispatcher = services.FirstOrDefault(s => s.ServiceType == typeof(IHandlrDispatcher));");
        builder.AppendLine("        if (existingDispatcher != null)");
        builder.AppendLine("            services.Remove(existingDispatcher);");
        builder.AppendLine("        services.AddScoped<IHandlrDispatcher, Handlr.Generated.GeneratedHandlrDispatcher>();");
        builder.AppendLine();

        // Register command handlers
        foreach (var handler in receiver.CommandHandlers)
        {
            var handlerInterface = handler.AllInterfaces.First(i =>
                i.OriginalDefinition.ToDisplayString() == "Handlr.Abstractions.Commands.ICommandHandler<TCommand>");
            var commandType = handlerInterface.TypeArguments[0];
            builder.AppendLine($"        services.AddScoped<ICommandHandler<{commandType.ToDisplayString()}>, {handler.ToDisplayString()}>();");
        }

        // Register command handlers with results
        foreach (var handler in receiver.CommandWithResultHandlers)
        {
            var handlerInterface = handler.AllInterfaces.First(i =>
                i.OriginalDefinition.ToDisplayString() == "Handlr.Abstractions.Commands.ICommandHandler<TCommand, TResult>");
            var commandType = handlerInterface.TypeArguments[0];
            var resultType = handlerInterface.TypeArguments[1];
            builder.AppendLine($"        services.AddScoped<ICommandHandler<{commandType.ToDisplayString()}, {resultType.ToDisplayString()}>, {handler.ToDisplayString()}>();");
        }

        // Register query handlers
        foreach (var handler in receiver.QueryHandlers)
        {
            var handlerInterface = handler.AllInterfaces.First(i =>
                i.OriginalDefinition.ToDisplayString() == "Handlr.Abstractions.Queries.IQueryHandler<TQuery, TResult>");
            var queryType = handlerInterface.TypeArguments[0];
            var resultType = handlerInterface.TypeArguments[1];
            builder.AppendLine($"        services.AddScoped<IQueryHandler<{queryType.ToDisplayString()}, {resultType.ToDisplayString()}>, {handler.ToDisplayString()}>();");
        }

        // Register FluentValidation validators (auto-discovered)
        foreach (var validator in receiver.Validators)
        {
            var validatorInterface = validator.AllInterfaces.First(i =>
                i.OriginalDefinition.ToDisplayString() == "FluentValidation.IValidator<T>");
            var validatedType = validatorInterface.TypeArguments[0];
            builder.AppendLine($"        services.AddScoped<FluentValidation.IValidator<{validatedType.ToDisplayString()}>, {validator.ToDisplayString()}>();");
        }

        builder.AppendLine();
        builder.AppendLine("        return services;");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static string GetResultType(ITypeSymbol typeSymbol)
    {
        var commandInterface = typeSymbol.AllInterfaces
            .FirstOrDefault(i => i.Name == "ICommand" && i.TypeArguments.Length == 1);

        if (commandInterface != null)
            return commandInterface.TypeArguments[0].ToDisplayString();

        var queryInterface = typeSymbol.AllInterfaces
            .FirstOrDefault(i => i.Name == "IQuery" && i.TypeArguments.Length == 1);

        if (queryInterface != null)
            return queryInterface.TypeArguments[0].ToDisplayString();

        return "object";
    }
}

internal sealed class HandlrSyntaxReceiver : ISyntaxContextReceiver
{
    public List<ITypeSymbol> Commands { get; } = new();
    public List<ITypeSymbol> CommandsWithResults { get; } = new();
    public List<ITypeSymbol> Queries { get; } = new();
    public List<ITypeSymbol> CommandHandlers { get; } = new();
    public List<ITypeSymbol> CommandWithResultHandlers { get; } = new();
    public List<ITypeSymbol> QueryHandlers { get; } = new();
    public List<ITypeSymbol> Validators { get; } = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not (RecordDeclarationSyntax or ClassDeclarationSyntax))
            return;

        if (context.SemanticModel.GetDeclaredSymbol(context.Node) is not INamedTypeSymbol typeSymbol)
            return;

        if (typeSymbol.IsAbstract)
            return;

        foreach (var @interface in typeSymbol.AllInterfaces)
        {
            var interfaceName = @interface.OriginalDefinition.ToDisplayString();

            switch (interfaceName)
            {
                case "Handlr.Abstractions.Commands.ICommand" when @interface.TypeArguments.Length == 0:
                    Commands.Add(typeSymbol);
                    break;

                case "Handlr.Abstractions.Commands.ICommand<TResult>" when @interface.TypeArguments.Length == 1:
                    CommandsWithResults.Add(typeSymbol);
                    break;

                case "Handlr.Abstractions.Queries.IQuery<TResult>" when @interface.TypeArguments.Length == 1:
                    Queries.Add(typeSymbol);
                    break;

                case "Handlr.Abstractions.Commands.ICommandHandler<TCommand>" when @interface.TypeArguments.Length == 1:
                    CommandHandlers.Add(typeSymbol);
                    break;

                case "Handlr.Abstractions.Commands.ICommandHandler<TCommand, TResult>" when @interface.TypeArguments.Length == 2:
                    CommandWithResultHandlers.Add(typeSymbol);
                    break;

                case "Handlr.Abstractions.Queries.IQueryHandler<TQuery, TResult>" when @interface.TypeArguments.Length == 2:
                    QueryHandlers.Add(typeSymbol);
                    break;

                case "FluentValidation.IValidator<T>" when @interface.TypeArguments.Length == 1:
                    Validators.Add(typeSymbol);
                    break;
            }
        }
    }
}

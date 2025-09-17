using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Handlr.Abstractions.Commands;
using Handlr.Abstractions.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Handlr.Abstractions.Common;

/// <summary>
/// Default implementation of IHandlrDispatcher that routes commands and queries to their handlers.
/// </summary>
/// <remarks>
/// Initializes a new instance of the HandlrDispatcher class.
/// </remarks>
/// <param name="serviceProvider">The service provider for resolving handlers</param>
public class HandlrDispatcher(IServiceProvider serviceProvider) : IHandlrDispatcher
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    /// <inheritdoc />
    public async Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        if (command == null) throw new ArgumentNullException(nameof(command));

        try
        {
            var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
            await handler.Handle(command, cancellationToken);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("No service"))
        {
            throw new InvalidOperationException(
                $"No handler registered for command '{typeof(TCommand).Name}'. " +
                "Make sure to register the handler using services.AddScoped<ICommandHandler<{commandType}>, {handlerType}>() " +
                "or install the Handlr.SourceGenerator package for automatic handler discovery.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        if (command == null) throw new ArgumentNullException(nameof(command));

        try
        {
            var commandType = command.GetType();
            var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));
            var handler = _serviceProvider.GetRequiredService(handlerType);
            var method = handlerType.GetMethod("Handle");
            var task = (Task<TResult>)method!.Invoke(handler, new object[] { command, cancellationToken })!;
            return await task;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("No service"))
        {
            throw new InvalidOperationException(
                $"No handler registered for command '{command.GetType().Name}'. " +
                "Make sure to register the handler using services.AddScoped<ICommandHandler<{commandType}, {resultType}>, {handlerType}>() " +
                "or install the Handlr.SourceGenerator package for automatic handler discovery.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<TResult> SendAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));

        try
        {
            var queryType = query.GetType();
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResult));
            var handler = _serviceProvider.GetRequiredService(handlerType);
            var method = handlerType.GetMethod("Handle");
            var task = (Task<TResult>)method!.Invoke(handler, new object[] { query, cancellationToken })!;
            return await task;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("No service"))
        {
            throw new InvalidOperationException(
                $"No handler registered for query '{query.GetType().Name}'. " +
                "Make sure to register the handler using services.AddScoped<IQueryHandler<{queryType}, {resultType}>, {handlerType}>() " +
                "or install the Handlr.SourceGenerator package for automatic handler discovery.", ex);
        }
    }
}

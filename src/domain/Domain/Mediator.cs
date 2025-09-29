using Domain.Abstraction.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Domain;

public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        var handlerInterface = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
        var handler = _serviceProvider.GetRequiredService(handlerInterface);

        var method = handlerInterface.GetMethod("HandleAsync");
        var result = await (Task<TResult>)method!.Invoke(handler, new object[] { command, cancellationToken })!;

        return result;
    }

    public async Task SendAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        var handlerInterface = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
        var handler = _serviceProvider.GetRequiredService(handlerInterface);

        var method = handlerInterface.GetMethod("HandleAsync");
        await (Task)method!.Invoke(handler, new object[] { command, cancellationToken })!;
    }

    public async Task<TResult> SendAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        var handlerInterface = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var handler = _serviceProvider.GetRequiredService(handlerInterface);

        var method = handlerInterface.GetMethod("HandleAsync");
        var result = await (Task<TResult>)method!.Invoke(handler, new object[] { query, cancellationToken })!;

        return result;
    }
}
using Domain.Abstraction.Mediator;

namespace Domain;

public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Send<TCommand>(TCommand command)
        where TCommand : ICommand<TCommand>
    {
        var handler = (ICommandHandler<TCommand>)_serviceProvider.GetService(typeof(ICommandHandler<TCommand>));
        if (handler == null)
        {
            throw new InvalidOperationException($"No handler found for command type {typeof(TCommand).FullName}");
        }
        handler.Handle(command);
    }

    public TResult Send<TCommand, TResult>(TCommand command)
        where TCommand : ICommand<TCommand, TResult>
    {
        var handler = (ICommandHandler<TCommand, TResult>)_serviceProvider.GetService(typeof(ICommandHandler<TCommand, TResult>));
        if (handler == null)
        {
            throw new InvalidOperationException($"No handler found for command type {typeof(TCommand).FullName}");
        }
        return handler.Handle(command);
    }
}


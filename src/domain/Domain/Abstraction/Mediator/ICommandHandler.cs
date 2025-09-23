namespace Domain.Abstraction.Mediator;

public interface ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TCommand, TResult>
{
    TResult Handle(TCommand command);
}

public interface ICommandHandler<TCommand>
    where TCommand : ICommand<TCommand>
{
    void Handle(TCommand command);
}
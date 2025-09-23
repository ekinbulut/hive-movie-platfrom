namespace Domain.Abstraction.Mediator;

public interface IMediator
{
    void Send<TCommand>(TCommand command)
        where TCommand : ICommand<TCommand>;

    TResult Send<TCommand, TResult>(TCommand command)
        where TCommand : ICommand<TCommand, TResult>;
}
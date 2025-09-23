namespace Domain.Abstraction.Mediator;

public interface ICommand<T, TResult>
    where T : ICommand<T, TResult>
{
}

public interface ICommand<T>
{
}
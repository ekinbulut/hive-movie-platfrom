namespace Domain.Events;

public interface IMessage
{
    string? CausationId { get; init; }
}
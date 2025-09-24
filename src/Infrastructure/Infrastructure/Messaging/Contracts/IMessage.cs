namespace Infrastructure.Messaging.Contracts;

public interface IMessage
{
    string? CausationId { get; init; }
}
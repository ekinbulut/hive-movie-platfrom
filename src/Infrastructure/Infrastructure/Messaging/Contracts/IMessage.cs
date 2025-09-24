namespace Infrastructure.Messaging.Contracts;

public interface IMessage
{
    string? CorrelationId { get; init; }
    string? CausationId { get; init; }
    DateTimeOffset CreatedAt { get; init; }
}
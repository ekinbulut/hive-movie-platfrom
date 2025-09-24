namespace Infrastructure.Messaging.Contracts;

public record MessageBase(
    string? CorrelationId,
    string? CausationId,
    DateTimeOffset CreatedAt
) : IMessage
{
    public static MessageBase New(string? correlationId = null, string? causationId = null)
        => new(correlationId ?? Guid.CreateVersion7().ToString("N"), causationId, DateTimeOffset.UtcNow);
}
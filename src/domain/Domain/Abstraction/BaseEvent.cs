namespace Domain.Abstraction;

public abstract class BaseEvent
{
    public Guid CorrelationId { get; private set; } = Guid.CreateVersion7();
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public string Meta { get; set; }
}
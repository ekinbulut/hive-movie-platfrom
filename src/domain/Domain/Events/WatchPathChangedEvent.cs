using Domain.Abstraction;

namespace Domain.Events;

public class WatchPathChangedEvent : BaseEvent, IMessage
{
    public string UserId { get; set; } = string.Empty;
    public string NewPath { get; set; } = string.Empty;
    public string? CausationId { get; init; }
}


using base_transport;
using Domain.Abstraction;

namespace Domain.Events;

public class WatchPathChangedEvent : BaseEvent, IMessage
{
    public Guid UserId { get; set; }
    public string NewPath { get; set; } = string.Empty;
    public string? CausationId { get; init; }
    public string CorrelationId { get; set; }
}


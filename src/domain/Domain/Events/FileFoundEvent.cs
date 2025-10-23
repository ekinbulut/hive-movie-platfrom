using base_transport;
using Domain.Abstraction;
using Domain.Models;

namespace Domain.Events;

public class FileFoundEvent : BaseEvent, IMessage
{
    public IEnumerable<string> FilePaths { get; set; }

    public FileFoundEvent()
    {
        FilePaths = new List<string>();
    }
    public string? CausationId { get; init; }

    public MetaData MetaData { get; set; }
    public Guid UserId { get; set; }
    public string CorrelationId { get; set; }
}
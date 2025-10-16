using Domain.Abstraction;
using Infrastructure.Messaging.Contracts;
using Watcher.Console.App.Models;

namespace Watcher.Console.App.Events;

public class FileFoundEvent : BaseEvent, IMessage
{
    public IEnumerable<string> FilePaths { get; set; }

    public FileFoundEvent()
    {
        FilePaths = new List<string>();
    }
    public string? CausationId { get; init; }

    public MetaData MetaData { get; set; }
}
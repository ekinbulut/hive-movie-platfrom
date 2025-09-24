using Domain.Abstraction;

namespace Watcher.Console.App.Events;

public class FileFoundEvent : BaseEvent
{
    public IEnumerable<string> FilePaths { get; set; }

    public FileFoundEvent()
    {
        FilePaths = new List<string>();
    }
}
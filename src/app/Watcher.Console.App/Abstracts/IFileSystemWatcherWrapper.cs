namespace Watcher.Console.App.Abstracts;

public interface IFileSystemWatcherWrapper : IDisposable
{
    string Path { get; set; }
    string Filter { get; set; }
    bool IncludeSubdirectories { get; set; }
    NotifyFilters NotifyFilter { get; set; }
    bool EnableRaisingEvents { get; set; }
    int InternalBufferSize { get; set; }

    event FileSystemEventHandler? Created;
    event FileSystemEventHandler? Changed;
    event FileSystemEventHandler? Deleted;
    event RenamedEventHandler? Renamed;
    event ErrorEventHandler? Error;

    void Start();
    void Stop();
}
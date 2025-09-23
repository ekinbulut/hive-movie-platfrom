namespace Watcher.Console.App.Abstracts;

public interface IFileSystemWatcherWrapper : IDisposable
{
    bool IncludeSubdirectories { get; set; }
    NotifyFilters NotifyFilter { get; set; }
    bool EnableRaisingEvents { get; set; }
    
    event FileSystemEventHandler? Created;
    event FileSystemEventHandler? Changed;
    event FileSystemEventHandler? Deleted;
    event RenamedEventHandler? Renamed;
    event ErrorEventHandler? Error;
}
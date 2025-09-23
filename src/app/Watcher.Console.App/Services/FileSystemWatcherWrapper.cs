using Watcher.Console.App.Abstracts;

namespace Watcher.Console.App.Services;

public class FileSystemWatcherWrapper : IFileSystemWatcherWrapper
{
    private readonly FileSystemWatcher _watcher;

    public FileSystemWatcherWrapper(FileSystemWatcher watcher)
    {
        _watcher = watcher;
    }

    public bool IncludeSubdirectories 
    { 
        get => _watcher.IncludeSubdirectories; 
        set => _watcher.IncludeSubdirectories = value; 
    }

    public NotifyFilters NotifyFilter 
    { 
        get => _watcher.NotifyFilter; 
        set => _watcher.NotifyFilter = value; 
    }

    public bool EnableRaisingEvents 
    { 
        get => _watcher.EnableRaisingEvents; 
        set => _watcher.EnableRaisingEvents = value; 
    }

    public event FileSystemEventHandler? Created
    {
        add => _watcher.Created += value;
        remove => _watcher.Created -= value;
    }

    public event FileSystemEventHandler? Changed
    {
        add => _watcher.Changed += value;
        remove => _watcher.Changed -= value;
    }

    public event FileSystemEventHandler? Deleted
    {
        add => _watcher.Deleted += value;
        remove => _watcher.Deleted -= value;
    }

    public event RenamedEventHandler? Renamed
    {
        add => _watcher.Renamed += value;
        remove => _watcher.Renamed -= value;
    }

    public event ErrorEventHandler? Error
    {
        add => _watcher.Error += value;
        remove => _watcher.Error -= value;
    }

    public void Dispose() => _watcher?.Dispose();
}
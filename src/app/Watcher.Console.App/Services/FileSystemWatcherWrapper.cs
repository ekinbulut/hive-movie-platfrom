using System.IO;
using Watcher.Console.App.Abstracts;

namespace Watcher.Console.App.Services;

public class FileSystemWatcherWrapper : IFileSystemWatcherWrapper
{
    private readonly FileSystemWatcher _watcher;
    private bool _disposed = false;

    public FileSystemWatcherWrapper(string path, string filter = "*.*")
    {
        _watcher = new FileSystemWatcher(path, filter)
        {
            // Increase buffer size to handle many simultaneous changes
            // Default is 8192 bytes, we're setting it to 64KB to handle bulk operations
            InternalBufferSize = 65536,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite,
            IncludeSubdirectories = true
        };

        // Wire up events
        _watcher.Created += (s, e) => Created?.Invoke(s, e);
        _watcher.Changed += (s, e) => Changed?.Invoke(s, e);
        _watcher.Deleted += (s, e) => Deleted?.Invoke(s, e);
        _watcher.Renamed += (s, e) => Renamed?.Invoke(s, e);
        _watcher.Error += OnError;
    }

    public string Path
    {
        get => _watcher.Path;
        set => _watcher.Path = value;
    }

    public string Filter
    {
        get => _watcher.Filter;
        set => _watcher.Filter = value;
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

    public int InternalBufferSize
    {
        get => _watcher.InternalBufferSize;
        set => _watcher.InternalBufferSize = value;
    }

    public event FileSystemEventHandler? Created;
    public event FileSystemEventHandler? Changed;
    public event FileSystemEventHandler? Deleted;
    public event RenamedEventHandler? Renamed;
    public event ErrorEventHandler? Error;

    private void OnError(object sender, ErrorEventArgs e)
    {
        // Handle the "Too many changes at once" error gracefully
        var exception = e.GetException();
        
        if (exception is InternalBufferOverflowException)
        {
            // Log the error and attempt to restart the watcher
            
            // Try to increase buffer size and restart
            try
            {
                _watcher.EnableRaisingEvents = false;
                
                // Double the buffer size up to a reasonable maximum (1MB)
                if (_watcher.InternalBufferSize < 1048576)
                {
                    _watcher.InternalBufferSize = Math.Min(_watcher.InternalBufferSize * 2, 1048576);
                }
                
                // Small delay before restarting
                Task.Delay(1000).ContinueWith(_ => {
                    if (!_disposed)
                    {
                        _watcher.EnableRaisingEvents = true;
                    }
                });
            }
            catch (Exception restartEx)
            {
                Error?.Invoke(sender, e);
            }
        }
        else
        {
            // For other errors, just propagate the event
            Error?.Invoke(sender, e);
        }
    }

    public void Start()
    {
        _watcher.EnableRaisingEvents = true;
    }

    public void Stop()
    {
        _watcher.EnableRaisingEvents = false;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _watcher?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
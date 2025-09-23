using Watcher.Console.App.Abstracts;
using Watcher.Console.App.Services;

namespace Watcher.Console.App.Factories;

public class FileSystemWatcherFactory : IFileSystemWatcherFactory
{
    public IFileSystemWatcherWrapper Create(string path, string filter)
    {
        return new FileSystemWatcherWrapper(new FileSystemWatcher(path, filter));
    }
}
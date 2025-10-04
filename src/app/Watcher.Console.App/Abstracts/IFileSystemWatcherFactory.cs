using Watcher.Console.App.Abstracts;

namespace Watcher.Console.App.Abstracts;

public interface IFileSystemWatcherFactory
{
    IFileSystemWatcherWrapper Create(string path, string filter = "*.*");
}
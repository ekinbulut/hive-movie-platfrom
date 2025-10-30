namespace Watcher.Console.App.Abstracts;

public interface IWatcherManager
{
    Task ChangeWatchPathAsync(string newPath, Guid userId);
    void StartWatcher(string path, string userId);
    void StopWatcher(string userId);
    void StopAllWatchers();
    Task StopAllWatchersAsync();
}


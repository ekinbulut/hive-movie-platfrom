namespace Watcher.Console.App.Abstracts;

public interface IStartupService
{
    Task InitializeWatchersAsync(CancellationToken cancellationToken = default);
    
    //Stop all watchers
    Task StopAllWatchersAsync();
}
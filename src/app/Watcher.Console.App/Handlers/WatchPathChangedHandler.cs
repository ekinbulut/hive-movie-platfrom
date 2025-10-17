using Domain.Events;
using Infrastructure.Messaging.Handlers;
using Watcher.Console.App.Abstracts;

namespace Watcher.Console.App.Handlers;

public class WatchPathChangedHandler(IWatcherManager watcherManager) : BaseMessageHandler<WatchPathChangedEvent>
{
    protected override async Task OnHandle(WatchPathChangedEvent message, string? causationId)
    {
        try
        {
            // Map the path from host to container path
            // Assuming /mnt/host is mounted as the base directory
            var containerPath = message.NewPath.StartsWith("/mnt/host")
                ? message.NewPath
                : $"/mnt/host/{message.NewPath.TrimStart('/')}";

            await watcherManager.ChangeWatchPathAsync(message.NewPath, message.UserId);
        }
        catch (Exception ex)
        {
            throw new DirectoryNotFoundException("Error changing watch path", ex);
        }
    }
}
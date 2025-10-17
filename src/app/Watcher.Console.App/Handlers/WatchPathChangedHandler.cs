using Domain.Events;
using Infrastructure.Messaging.Handlers;
using Microsoft.Extensions.Logging;
using Watcher.Console.App.Abstracts;

namespace Watcher.Console.App.Handlers;

public class WatchPathChangedHandler(IWatcherManager watcherManager, ILogger<WatchPathChangedHandler> logger) : BaseMessageHandler<WatchPathChangedEvent>
{
    protected override async Task OnHandle(WatchPathChangedEvent message, string? causationId)
    {
        try
        {
            logger.LogInformation($"Handling {nameof(WatchPathChangedEvent)} with userId: {message.UserId} and newPath: {message.NewPath}");
            
            // Map the path from host to container path
            // Assuming /mnt/host is mounted as the base directory
            var containerPath = message.NewPath.StartsWith("/mnt/host")
                ? message.NewPath
                : $"/mnt/host/{message.NewPath.TrimStart('/')}";

            await watcherManager.ChangeWatchPathAsync(containerPath, message.UserId);
        }
        catch (Exception ex)
        {
            throw new DirectoryNotFoundException("Error changing watch path", ex);
        }
    }
}
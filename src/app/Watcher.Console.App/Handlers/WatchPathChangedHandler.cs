using Domain.Events;
using Infrastructure.Messaging.Handlers;
using Microsoft.Extensions.Logging;
using Watcher.Console.App.Abstracts;
using Watcher.Console.App.Events;

namespace Watcher.Console.App.Handlers;

public class WatchPathChangedHandler : BaseMessageHandler<WatchPathChangedEvent>
{
    private readonly ILogger<WatchPathChangedHandler> _logger;
    private readonly IWatcherManager _watcherManager;

    public WatchPathChangedHandler(
        ILogger<WatchPathChangedHandler> logger,
        IWatcherManager watcherManager)
    {
        _logger = logger;
        _watcherManager = watcherManager;
    }

    protected override async Task OnHandle(WatchPathChangedEvent message, string? causationId)
    {
        _logger.LogInformation(
            "Received WatchPathChangedEvent for UserId: {UserId}, NewPath: {NewPath}",
            message.UserId,
            message.NewPath);

        try
        {
            // Map the path from host to container path
            // Assuming /mnt/host is mounted as the base directory
            var containerPath = message.NewPath.StartsWith("/mnt/host")
                ? message.NewPath
                : $"/mnt/host/{message.NewPath.TrimStart('/')}";

            await _watcherManager.ChangeWatchPathAsync(containerPath, message.UserId);

            _logger.LogInformation(
                "Successfully changed watch path to {Path} for user {UserId}",
                containerPath,
                message.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to change watch path to {Path} for user {UserId}",
                message.NewPath,
                message.UserId);
            throw;
        }
    }
}

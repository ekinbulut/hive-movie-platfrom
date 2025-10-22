using base_transport;
using Domain.Events;
using Microsoft.Extensions.Logging;
using Watcher.Console.App.Abstracts;

namespace Watcher.Console.App.Handlers;

public class WatchPathChangedHandler(
    IWatcherManager watcherManager,
    ILogger<WatchPathChangedHandler> logger,
    IBasicMessagingService messagingService)
{
    private const string Queue = "config.changed";
    
    public async Task StartListeningAsync(CancellationToken ct = default)
    {
        await messagingService.ConnectAsync(ct);

        messagingService.ReceivedAsync += async (sender, args) =>
        {
            var body = args.Body.ToArray();
            var messageString = System.Text.Encoding.UTF8.GetString(body);
            var watchPathChangedEvent = System.Text.Json.JsonSerializer.Deserialize<WatchPathChangedEvent>(messageString);
            
            if (watchPathChangedEvent != null)
            {
                await HandleAsync(watchPathChangedEvent);
            }
        };

        await messagingService.BasicConsumeAsync(Queue, autoAck: true, ct);
    }

    private async Task HandleAsync(WatchPathChangedEvent message)
    {
        try
        {
            logger.LogInformation($"Handling {nameof(WatchPathChangedEvent)} with userId: {message.UserId} and newPath: {message.NewPath}");

            var containerPath = message.NewPath.StartsWith("/mnt/host")
                ? message.NewPath
                : $"/mnt/host/{message.NewPath.TrimStart('/')}";

            await watcherManager.ChangeWatchPathAsync(containerPath, message.UserId);
            
            logger.LogInformation($"Successfully changed watch path to {containerPath} for user {message.UserId}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error changing watch path for user {message.UserId}");
            throw new DirectoryNotFoundException("Error changing watch path", ex);
        }
    }
}

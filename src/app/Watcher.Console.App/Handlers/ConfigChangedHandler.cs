using base_transport;
using Domain.Events;
using Microsoft.Extensions.Logging;
using Watcher.Console.App.Abstracts;

namespace Watcher.Console.App.Handlers;

public class ConfigChangedHandler(
    IWatcherManager watcherManager,
    ILogger<ConfigChangedHandler> logger,
    IBasicMessagingService service)
    : BaseMessageHandler<WatchPathChangedEvent>(service)
{
    private readonly IBasicMessagingService _messagingService = service;

    public override async Task HandleAsync(WatchPathChangedEvent message, ulong deliveryTag,
        CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            logger.LogInformation(
                $"Handling {nameof(WatchPathChangedEvent)} with userId: {message.UserId} and newPath: {message.NewPath}");

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
        finally
        {
            await _messagingService.AcknowledgeMessageAsync(deliveryTag, cancellationToken);
        }
    }
}
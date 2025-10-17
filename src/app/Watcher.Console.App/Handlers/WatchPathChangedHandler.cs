using Domain.Events;
using Domain.Interfaces;
using Infrastructure.Integration.Services.JellyFin;
using Infrastructure.Messaging.Handlers;
using Microsoft.Extensions.Logging;
using Watcher.Console.App.Abstracts;
using Watcher.Console.App.Events;

namespace Watcher.Console.App.Handlers;

public class WatchPathChangedHandler(
    ILogger<WatchPathChangedHandler> logger,
    IWatcherManager watcherManager, 
    IConfigurationRepository configurationRepository,
    IJellyFinServiceConfiguration jellyFinServiceConfiguration)
    : BaseMessageHandler<WatchPathChangedEvent>
{
    protected override async Task OnHandle(WatchPathChangedEvent message, string? causationId)
    {
        logger.LogInformation(
            "Received WatchPathChangedEvent for UserId: {UserId}, NewPath: {NewPath}",
            message.UserId,
            message.NewPath);

        try
        {
            
            //get configuration for user
            var config = await configurationRepository.GetConfigurationByUserIdAsync(message.UserId);
            if (config == null)
            {
                logger.LogWarning(
                    "No configuration found for UserId: {UserId}. Cannot change watch path.",
                    message.UserId);
                return;
            }
            //update jellyfin service configuration
            jellyFinServiceConfiguration.ApiKey = config.Settings.JellyFinApiKey;
            jellyFinServiceConfiguration.BaseUrl = config.Settings.JellyFinServer;
            
            // Map the path from host to container path
            // Assuming /mnt/host is mounted as the base directory
            var containerPath = message.NewPath.StartsWith("/mnt/host")
                ? message.NewPath
                : $"/mnt/host/{message.NewPath.TrimStart('/')}";

            await watcherManager.ChangeWatchPathAsync(containerPath, message.UserId);

            logger.LogInformation(
                "Successfully changed watch path to {Path} for user {UserId}",
                containerPath,
                message.UserId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to change watch path to {Path} for user {UserId}",
                message.NewPath,
                message.UserId);
            throw;
        }
    }
}

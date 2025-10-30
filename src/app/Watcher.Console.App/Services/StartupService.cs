using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Watcher.Console.App.Abstracts;

namespace Watcher.Console.App.Services;

public class StartupService(IUserRepository userRepository, IConfigurationRepository configurationRepository,  ILogger<StartupService> logger, IWatcherManager watcherManager) : IStartupService
{
    public async Task InitializeWatchersAsync(CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetAllAsync();
        
        foreach (var user in users)
        {
            var config = await configurationRepository.GetConfigurationByUserIdAsync(user.Id, cancellationToken);
            if (config != null)
            {
                var watchPath = config.Settings.MediaFolder;
                try
                {
                    await watcherManager.ChangeWatchPathAsync(watchPath, user.Id);
                    logger.LogInformation($"Initialized watcher for user {user.Id} at path {watchPath}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to initialize watcher for user {user.Id} at path {watchPath}");
                }
            }
            else
            {
                logger.LogWarning($"No configuration found for user {user.Id}. Watcher not initialized.");
            }
        }
    }

    public Task StopAllWatchersAsync()
    {
        return watcherManager.StopAllWatchersAsync();
    }
}
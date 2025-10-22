using base_transport;
using Domain.Abstraction.Mediator;
using Domain.Events;
using Domain.Interfaces;

namespace Hive.Idm.Api.Endpoints.Configuration.Add;

public class AddUserConfigCommandHandler(IConfigurationRepository configurationRepository, IBasicMessagingService basicMessagingService)
    : ICommandHandler<AddUserConfigurationCommand, bool>
{
    public async Task<bool> HandleAsync(AddUserConfigurationCommand command,
        CancellationToken cancellationToken = default)
    {
        var newConfig = new Domain.Entities.Configuration
        {
            UserId = command.UserId,
            Settings = command.Settings
        };

        var config = await configurationRepository.GetConfigurationByUserIdAsync(command.UserId, cancellationToken);

        var response = false;
        if (config == null)
        {
             response = await configurationRepository.AddConfigurationAsync(newConfig, cancellationToken);
        }
        else
        {
            config.Settings = newConfig.Settings;
            response =  await configurationRepository.UpdateConfigurationAsync(config, cancellationToken);
        }

        if (!response) return response;
        var @event = new WatchPathChangedEvent()
        {
            UserId = command.UserId,
            NewPath = command.Settings.MediaFolder,
            CausationId = Guid.CreateVersion7().ToString()
        };
        await basicMessagingService.ConnectAsync(cancellationToken);
        await basicMessagingService.BasicPublishAsync("config.changed",
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(@event), cancellationToken);
        
        return response;
    }
}
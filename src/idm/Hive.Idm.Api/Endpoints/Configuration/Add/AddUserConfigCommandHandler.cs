using Domain.Abstraction.Mediator;
using Domain.Interfaces;
using Hive.Idm.Api.Endpoints.Configuration.Get;

namespace Hive.Idm.Api.Endpoints.Configuration.Add;

public class AddUserConfigCommandHandler(IConfigurationRepository configurationRepository)
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

        if (config == null)
        {
            return await configurationRepository.AddConfigurationAsync(newConfig, cancellationToken);
        }

        config.Settings = newConfig.Settings;
        return  await configurationRepository.UpdateConfigurationAsync(config, cancellationToken);
    }
}
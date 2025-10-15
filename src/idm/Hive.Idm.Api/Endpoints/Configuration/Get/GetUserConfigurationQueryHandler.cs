using Domain.Abstraction.Mediator;
using Domain.DTOs;
using Domain.Interfaces;

namespace Hive.Idm.Api.Endpoints.Configuration.Get;

public class GetUserConfigurationQueryHandler(IConfigurationRepository configurationRepository) 
    : IQueryHandler<GetUserConfigurationQuery, GetUserConfigurationResponse?>
{
    public async Task<GetUserConfigurationResponse?> HandleAsync(GetUserConfigurationQuery query,
        CancellationToken cancellationToken = default)
    {
        var config = await configurationRepository.GetConfigurationByUserIdAsync(query.UserId, cancellationToken);
        
        return config == null ? null 
            : new GetUserConfigurationResponse()
            {
                Settings = new SettingsDTO()
                {
                    // Map properties from config to SettingsDTO here
                    JellyFinServer = config.Settings.JellyFinServer,
                    JellyFinApiKey = config.Settings.JellyFinApiKey,
                    MediaFolder = config.Settings.MediaFolder
                }
            };
    }
}
using System.Security.Claims;
using Domain.Abstraction.Mediator;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Hive.Idm.Api.Endpoints.Configuration.Add;

public class AddUserConfigEndpoint(IMediator mediator) : Endpoint<AddUserConfigurationRequest, bool>
{
    
    public override void Configure()
    {
        Post("/user/configuration");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(AddUserConfigurationRequest req, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        
        var command = new AddUserConfigurationCommand
        {
            UserId = userId,
            Settings = new Domain.Entities.Settings
            {
                JellyFinServer = req.JellyFinServer,
                JellyFinApiKey = req.JellyFinApiKey,
                MediaFolder = req.MediaFolder
            }
        };
        
        var response = await mediator.SendAsync(command, ct);
        
        if (!response)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(response, ct);
    }
}
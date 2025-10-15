using System.Security.Claims;
using Domain.Abstraction.Mediator;
using Domain.Entities;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Hive.Idm.Api.Endpoints.Configuration.Get;

public class GetUserConfigEndpoint(IMediator mediator) : EndpointWithoutRequest<GetUserConfigurationResponse>
{
    public override void Configure()
    {
        Get("/user/configuration");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get user ID from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        
        var query = new GetUserConfigurationQuery { UserId = userId };
        var response = await mediator.SendAsync(query, ct);

        if (response == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        
        await Send.OkAsync(response, ct);
    }
}

// handler
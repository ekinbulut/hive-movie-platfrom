using System.Security.Claims;
using Domain.Abstraction.Mediator;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Hive.Idm.Api.Endpoints.Info;

public class GetUserInfoEndpoint(IMediator mediator) : EndpointWithoutRequest<GetUserInfoResponse>
{
    public override void Configure()
    {
        Get("/user/info");
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
        
        var query = new GetUserInfoQuery { UserId = userId };
        var response = await mediator.SendAsync(query, ct);
        await Send.OkAsync(response, ct);
    }
}
using System.Security.Claims;
using Domain.Abstraction.Mediator;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Hive.Idm.Api.Endpoints.Info.Update;

public class PutUserInfoEndpoint(IMediator mediator) : Endpoint<PutUserInfoRequest>
{
    public override void Configure()
    {
        Put("/user/info");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

    }

    public override async Task HandleAsync(PutUserInfoRequest req, CancellationToken ct)
    {
        // Get user ID from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        
        var command = new UpdateUserInfoCommand
        {
            UserId = userId,
            FirstName = req.FirstName,
            LastName = req.LastName
        };
        
        var result = await mediator.SendAsync(command, ct);
        if (!result)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(new {message = "User info updated successfully" }, ct);
    }
    
}
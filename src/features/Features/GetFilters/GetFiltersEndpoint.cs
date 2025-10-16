using System.Security.Claims;
using Domain.Abstraction.Mediator;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Features.GetFilters;

public class GetFiltersEndpoint(IMediator mediator) : EndpointWithoutRequest<GetFiltersResponse>
{
    public override void Configure()
    {
        Get("/filters");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        
        var query = new GetFiltersQuery()
        {
            UserId = userId
        };
        var response = await mediator.SendAsync(query, ct);
        await Send.OkAsync(response, ct);
    }
}
using System.Security.Claims;
using Domain.Abstraction.Mediator;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Features.GetAllMovies;

[Authorize]
public class GetAllMoviesEndpoint(IMediator mediator) : Endpoint<GetMoviesRequest, GetMovieResponse>
{
    public override void Configure()
    {
        Post("/movies");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetMoviesRequest req, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        
        var query = new GetAllMoviesQuery(req.pageNumber, req.pageSize, userId);

        var response = await mediator.SendAsync(query, ct);
        await Send.OkAsync(response, ct);
    }
}
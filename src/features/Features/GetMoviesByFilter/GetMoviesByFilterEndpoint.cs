using System.Security.Claims;
using Domain.Abstraction.Mediator;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Features.GetMoviesByFilter;

public class GetMoviesByFilterEndpoint(IMediator mediator) : Endpoint<GetMoviesByFilterRequest, GetMoviesByFilterResponse>
{
    public override void Configure()
    {
        Post("/movies/filter");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

    }
    public override async Task HandleAsync(GetMoviesByFilterRequest req, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        
        var query = new GetMoviesByFilterQuery()
        {
            Year = req.Year,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            UserId = userId
        };

        var response = await mediator.SendAsync(query, ct);
        await Send.OkAsync(response, ct);
    }
}
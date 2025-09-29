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
        
        var query = new GetAllMoviesQuery(req.pageNumber, req.pageSize);

        var response = await mediator.SendAsync(query, ct);
        await Send.OkAsync(response, ct);
    }
}
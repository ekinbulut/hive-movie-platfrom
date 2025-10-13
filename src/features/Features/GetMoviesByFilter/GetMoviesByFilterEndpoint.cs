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
        var query = new GetMoviesByFilterQuery()
        {
            Year = req.Year,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize
        };

        var response = await mediator.SendAsync(query, ct);
        await Send.OkAsync(response, ct);
    }
}
using System.Runtime.InteropServices.ComTypes;
using Domain.Abstraction.Mediator;
using Domain.Interfaces;
using FastEndpoints;
using Features.GetMoviesByFilter;
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
        var query = new GetFiltersQuery();
        var response = await mediator.SendAsync(query, ct);
        await Send.OkAsync(response, ct);
    }
}

public class GetFiltersQuery : IQuery<GetFiltersResponse>
{
    
}

public class GetFiltersResponse
{
    public Filters Filters { get; set; }
}

public class GetFiltersQueryHandler(IMovieRepository movieRepository) : IQueryHandler<GetFiltersQuery, GetFiltersResponse>
{
    public async Task<GetFiltersResponse> HandleAsync(GetFiltersQuery query, CancellationToken cancellationToken = default)
    {
        var years = await movieRepository.GetFiltersAsync();
        
        return await Task.FromResult(new GetFiltersResponse()
        {
            Filters = new Filters()
            {
                Years = years
            }
        });
    }
}
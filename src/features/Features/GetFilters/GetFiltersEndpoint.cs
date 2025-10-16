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
        var query = new GetFiltersQuery();
        var response = await mediator.SendAsync(query, ct);
        await Send.OkAsync(response, ct);
    }
}
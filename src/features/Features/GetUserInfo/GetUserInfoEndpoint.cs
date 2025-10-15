using Domain.Abstraction.Mediator;
using FastEndpoints;

namespace Features.GetUserInfo;

public class GetUserInfoEndpoint(IMediator mediator) : EndpointWithoutRequest<GetUserInfoResponse>
{
    public override void Configure()
    {
        Get("/user/info");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var query = new GetUserInfoQuery();
        var response = await mediator.SendAsync(query, ct);
        await Send.OkAsync(response, ct);
    }
}


public class GetUserInfoQueryHandler : IQueryHandler<GetUserInfoQuery, GetUserInfoResponse>
{
    public async Task<GetUserInfoResponse> HandleAsync(GetUserInfoQuery query,
        CancellationToken cancellationToken = default)
    {
        // Mock user info - replace with actual user service
        return await Task.FromResult(new GetUserInfoResponse()
        {
            Name = "John",
            Surname = "Doe",
            Email = ""
        });
    }
}


public class GetUserInfoQuery : IQuery<GetUserInfoResponse>
{
    
}

public class GetUserInfoResponse
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
}
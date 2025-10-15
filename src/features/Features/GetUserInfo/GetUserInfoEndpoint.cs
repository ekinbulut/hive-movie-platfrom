using Domain.Abstraction.Mediator;
using Domain.Interfaces;
using FastEndpoints;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Features.GetUserInfo;

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


public class GetUserInfoQueryHandler(IUserRepository userRepository) : IQueryHandler<GetUserInfoQuery, GetUserInfoResponse>
{
    public async Task<GetUserInfoResponse> HandleAsync(GetUserInfoQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(query.UserId);
        
        if (user == null)
        {
            return new GetUserInfoResponse
            {
                Name = string.Empty,
                Surname = string.Empty,
                Email = string.Empty
            };
        }
        
        return new GetUserInfoResponse
        {
            Name = user.FirstName ?? string.Empty,
            Surname = user.LastName ?? string.Empty,
            Email = user.Email
        };
    }
}


public class GetUserInfoQuery : IQuery<GetUserInfoResponse>
{
    public Guid UserId { get; set; }
}

public class GetUserInfoResponse
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
}
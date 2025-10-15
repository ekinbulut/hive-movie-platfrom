using Domain.Abstraction.Mediator;
using Domain.Interfaces;

namespace Hive.Idm.Api.Endpoints.Info;

public class GetUserInfoQueryHandler(IUserRepository userRepository) : IQueryHandler<GetUserInfoQuery, GetUserInfoResponse?>
{
    public async Task<GetUserInfoResponse?> HandleAsync(GetUserInfoQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(query.UserId);
        
        return user == null ? null 
            : new GetUserInfoResponse(user.FirstName ?? string.Empty, 
                user.LastName ?? string.Empty, 
                user.Email);
    }
}
using Domain.Abstraction.Mediator;
using Domain.Interfaces;

namespace Features.GetUserInfo;

public class GetUserInfoQueryHandler(IUserRepository userRepository) : IQueryHandler<GetUserInfoQuery, GetUserInfoResponse>
{
    public async Task<GetUserInfoResponse> HandleAsync(GetUserInfoQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(query.UserId);
        
        if (user == null)
        {
            return new GetUserInfoResponse(string.Empty, string.Empty, string.Empty);
        }
        
        return new GetUserInfoResponse(user.FirstName ?? string.Empty, user.LastName ?? string.Empty, user.Email ?? string.Empty);

    }
}
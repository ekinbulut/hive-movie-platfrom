using Domain.Abstraction.Mediator;

namespace Features.GetUserInfo;

public class GetUserInfoQuery : IQuery<GetUserInfoResponse>
{
    public Guid UserId { get; set; }
}
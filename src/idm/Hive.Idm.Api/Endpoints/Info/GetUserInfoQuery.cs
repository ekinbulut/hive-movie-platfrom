using Domain.Abstraction.Mediator;

namespace Hive.Idm.Api.Endpoints.Info;

public class GetUserInfoQuery : IQuery<GetUserInfoResponse>
{
    public Guid UserId { get; set; }
}
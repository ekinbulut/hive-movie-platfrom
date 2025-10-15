using Domain.Abstraction.Mediator;

namespace Hive.Idm.Api.Endpoints.Info.Get;

public class GetUserInfoQuery : IQuery<GetUserInfoResponse>
{
    public Guid UserId { get; set; }
}
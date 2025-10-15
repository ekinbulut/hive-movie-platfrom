using Domain.Abstraction.Mediator;

namespace Hive.Idm.Api.Endpoints.Configuration.Get;

public class GetUserConfigurationQuery : IQuery<GetUserConfigurationResponse>
{
    public Guid UserId { get; set; }
}
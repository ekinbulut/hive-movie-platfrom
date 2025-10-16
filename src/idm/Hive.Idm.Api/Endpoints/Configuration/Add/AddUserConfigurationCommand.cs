using Domain.Abstraction.Mediator;

namespace Hive.Idm.Api.Endpoints.Configuration.Add;

public class AddUserConfigurationCommand : ICommand<bool>
{
    public Guid UserId { get; set; }
    public Domain.Entities.Settings Settings { get; set; }

}
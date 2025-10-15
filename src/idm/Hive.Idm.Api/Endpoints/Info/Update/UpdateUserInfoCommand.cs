using Domain.Abstraction.Mediator;

namespace Hive.Idm.Api.Endpoints.Info.Update;

public class UpdateUserInfoCommand : ICommand<bool>
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
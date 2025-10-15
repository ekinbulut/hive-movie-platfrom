using Domain.Abstraction.Mediator;
using Domain.Interfaces;

namespace Hive.Idm.Api.Endpoints.Info.Update;

public class PutUserInfoCommandHandler(IUserRepository userRepository) : ICommandHandler<UpdateUserInfoCommand, bool>
{
    public Task<bool> HandleAsync(UpdateUserInfoCommand command, CancellationToken cancellationToken = default)
    {
        return userRepository.UpdateUserInfoAsync(command.UserId, command.FirstName, command.LastName, cancellationToken);
    }
}
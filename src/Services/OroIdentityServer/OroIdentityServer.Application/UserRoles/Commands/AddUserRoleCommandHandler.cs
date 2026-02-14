namespace OroIdentityServer.Application.Commands;

public class AddUserRoleCommandHandler(
    ILogger<AddUserRoleCommandHandler> logger,
    IUserRolesRepository repository)
    : ICommandHandler<AddUserRoleCommand>
{
    public async Task HandleAsync(AddUserRoleCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling AddUserRoleCommand for UserId: {UserId}", command.UserId.Value);
        var userRole = new UserRole(command.UserId, command.RoleId);
        await repository.AddUserRoleAsync(userRole, cancellationToken);
        logger.LogInformation("UserRole added for UserId: {UserId}", command.UserId.Value);
    }
}
namespace OroIdentityServer.Application.Commands;

public class DeleteUserRoleCommandHandler(
    ILogger<DeleteUserRoleCommandHandler> logger,
    IUserRolesRepository repository)
    : ICommandHandler<DeleteUserRoleCommand>
{
    public async Task HandleAsync(DeleteUserRoleCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling DeleteUserRoleCommand for UserId: {UserId}", command.UserId.Value);
        var userRole = new UserRole(command.UserId, command.RoleId);
        await repository.DeleteUserRoleAsync(userRole, cancellationToken);
        logger.LogInformation("UserRole deleted for UserId: {UserId}", command.UserId.Value);
    }
}
namespace OroIdentityServer.Application.Commands;

public class DeleteRolesByUserIdCommandHandler(
    ILogger<DeleteRolesByUserIdCommandHandler> logger,
    IUserRolesRepository repository)
    : ICommandHandler<DeleteRolesByUserIdCommand>
{
    public async Task HandleAsync(DeleteRolesByUserIdCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling DeleteRolesByUserIdCommand for UserId: {UserId}", command.UserId.Value);
        await repository.DeleteRolesByUserIdAsync(command.UserId, cancellationToken);
        logger.LogInformation("Deleted roles for UserId: {UserId}", command.UserId.Value);
    }
}
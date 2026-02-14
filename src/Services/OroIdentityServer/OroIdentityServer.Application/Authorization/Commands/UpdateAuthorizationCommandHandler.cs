namespace OroIdentityServer.Application.Commands;

public class UpdateAuthorizationCommandHandler(
    ILogger<UpdateAuthorizationCommandHandler> logger,
    IAuthorizationRepository repository)
    : ICommandHandler<UpdateAuthorizationCommand>
{
    public async Task HandleAsync(UpdateAuthorizationCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling UpdateAuthorizationCommand for Id: {Id}", command.Id);
        var auth = await repository.GetAuthorizationByAsync(command.Id, cancellationToken);
        if (auth == null)
            throw new InvalidOperationException("Authorization not found.");

        if (!string.IsNullOrWhiteSpace(command.NewStatus))
            auth.UpdateStatus(command.NewStatus);

        if (command.NewProperties != null)
            auth.UpdateProperties(command.NewProperties);

        await repository.UpdateAuthorizationAsync(auth, cancellationToken);
        logger.LogInformation("Authorization updated: {Id}", command.Id);
    }
}
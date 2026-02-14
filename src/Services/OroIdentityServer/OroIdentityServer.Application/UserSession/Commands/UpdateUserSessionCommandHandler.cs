namespace OroIdentityServer.Application.Commands;

public class UpdateUserSessionCommandHandler(
    ILogger<UpdateUserSessionCommandHandler> logger,
    IUserSessionRepository repository)
    : ICommandHandler<UpdateUserSessionCommand>
{
    public async Task HandleAsync(UpdateUserSessionCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling UpdateUserSessionCommand for refresh token");
        var session = await repository.GetUserSessionByRefreshTokenAsync(command.RefreshToken, cancellationToken);
        if (session == null)
        {
            logger.LogWarning("UserSession not found for refresh token");
            throw new InvalidOperationException("UserSession not found.");
        }

        if (command.Deactivate == true)
            session.Deactivate();

        await repository.UpdateUserSessionAsync(session, cancellationToken);
        logger.LogInformation("UserSession updated for refresh token");
    }
}
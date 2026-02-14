namespace OroIdentityServer.Application.Commands;

public class RotateRefreshTokenCommandHandler(
    ILogger<RotateRefreshTokenCommandHandler> logger,
    IUserSessionRepository repository)
    : ICommandHandler<RotateRefreshTokenCommand>
{
    public async Task HandleAsync(RotateRefreshTokenCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling RotateRefreshTokenCommand");
        var session = await repository.GetUserSessionByRefreshTokenAsync(command.OldRefreshToken, cancellationToken);
        if (session == null)
        {
            logger.LogWarning("UserSession not found for refresh token");
            throw new InvalidOperationException("UserSession not found.");
        }

        session.RotateRefreshToken(command.NewRefreshToken);
        await repository.UpdateUserSessionAsync(session, cancellationToken);
        logger.LogInformation("Rotated refresh token for session {sessionId}", session.Id.Value);
    }
}

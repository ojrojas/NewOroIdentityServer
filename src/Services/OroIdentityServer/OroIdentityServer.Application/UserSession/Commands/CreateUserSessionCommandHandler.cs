namespace OroIdentityServer.Application.Commands;

public class CreateUserSessionCommandHandler(
    ILogger<CreateUserSessionCommandHandler> logger,
    IUserSessionRepository repository)
    : ICommandHandler<CreateUserSessionCommand>
{
    public async Task HandleAsync(CreateUserSessionCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling CreateUserSessionCommand for UserId: {UserId}", command.UserId.Value);
        var session = new UserSession(
            null,
            command.UserId,
            command.RefreshToken,
            command.IpAddress,
            command.Country,
            command.LoginTime,
            command.ExpiresAt);

        await repository.AddUserSessionAsync(session, cancellationToken);
        logger.LogInformation("UserSession created for UserId: {UserId}", command.UserId.Value);
    }
}
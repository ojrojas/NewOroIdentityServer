namespace OroIdentityServer.Application.Commands;

public class CreateConsentCommandHandler(
    ILogger<CreateConsentCommandHandler> logger,
    IUserConsentRepository repository)
    : ICommandHandler<CreateConsentCommand>
{
    public async Task HandleAsync(CreateConsentCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating consent for user {UserId} app {AppId}", command.UserId.Value, command.ApplicationId.Value);
        var consent = new Consent(ConsentId.New(), command.UserId, command.ApplicationId, command.Scopes, DateTime.UtcNow, command.Remember);
        await repository.AddConsentAsync(consent, cancellationToken);
    }
}

namespace OroIdentityServer.Application.Commands;

public class CreateAuthorizationCommandHandler(
    ILogger<CreateAuthorizationCommandHandler> logger,
    IApplicationRepository applicationRepository,
    IAuthorizationRepository authorizationRepository)
    : ICommandHandler<CreateAuthorizationCommand>
{
    public async Task HandleAsync(CreateAuthorizationCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling CreateAuthorizationCommand for ApplicationId: {AppId}", command.ApplicationId.Value);
        var app = await applicationRepository.GetByIdAsync(command.ApplicationId, cancellationToken);
        if (app == null)
            throw new InvalidOperationException("Application not found.");

        var authorization = new Core.Models.Authorization(app, command.ExpiresAt, command.Scopes, command.Status, command.Type, command.Properties);
        await authorizationRepository.AddAuthorizationAsync(authorization, cancellationToken);
        logger.LogInformation("Created authorization for ApplicationId: {AppId}", command.ApplicationId.Value);
    }
}
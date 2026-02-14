namespace OroIdentityServer.Application.Commands;

public class UpdateApplicationCommandHandler(
    ILogger<UpdateApplicationCommandHandler> logger,
    IApplicationRepository repository)
    : ICommandHandler<UpdateApplicationCommand>
{
    public async Task HandleAsync(UpdateApplicationCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling UpdateApplicationCommand for ApplicationId: {Id}", command.Application.Id.Value);
        await repository.UpdateAsync(command.Application, cancellationToken);
        logger.LogInformation("Handled UpdateApplicationCommand successfully for ApplicationId: {Id}", command.Application.Id.Value);
    }
}
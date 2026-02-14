namespace OroIdentityServer.Application.Commands;

public class DeleteApplicationCommandHandler(
    ILogger<DeleteApplicationCommandHandler> logger,
    IApplicationRepository repository)
    : ICommandHandler<DeleteApplicationCommand>
{
    public async Task HandleAsync(DeleteApplicationCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling DeleteApplicationCommand for ApplicationId: {Id}", command.Id.Value);
        var app = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (app == null)
            throw new InvalidOperationException("Application not found.");

        await repository.DeleteAsync(app, cancellationToken);
        logger.LogInformation("Deleted Application: {Id}", command.Id.Value);
    }
}
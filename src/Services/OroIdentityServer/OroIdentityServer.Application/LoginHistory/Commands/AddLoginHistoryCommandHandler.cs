namespace OroIdentityServer.Application.Commands;

public class AddLoginHistoryCommandHandler(
    ILogger<AddLoginHistoryCommandHandler> logger,
    ILoginHistoryRepository repository)
    : ICommandHandler<AddLoginHistoryCommand>
{
    public async Task HandleAsync(AddLoginHistoryCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling AddLoginHistoryCommand for UserId: {UserId}", command.UserId.Value);
        var entity = new LoginHistory(null, command.UserId, command.IpAddress, command.Country, command.LoginTime);
        await repository.AddLoginHistoryAsync(entity, cancellationToken);
        logger.LogInformation("LoginHistory added for UserId: {UserId}", command.UserId.Value);
    }
}
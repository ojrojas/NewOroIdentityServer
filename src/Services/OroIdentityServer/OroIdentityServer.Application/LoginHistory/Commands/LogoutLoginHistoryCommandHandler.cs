namespace OroIdentityServer.Application.Commands;

public class LogoutLoginHistoryCommandHandler(
    ILogger<LogoutLoginHistoryCommandHandler> logger,
    ILoginHistoryRepository loginHistoryRepository)
    : ICommandHandler<LogoutLoginHistoryCommand>
{
    public async Task HandleAsync(LogoutLoginHistoryCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling LogoutLoginHistoryCommand for Id: {Id}", command.Id.Value);
        var existing = await loginHistoryRepository.GetLoginHistoryByIdAsync(command.Id, cancellationToken);
        if (existing == null)
        {
            logger.LogWarning("LoginHistory not found: {Id}", command.Id.Value);
            throw new InvalidOperationException("LoginHistory not found.");
        }

        existing.Logout();
        await loginHistoryRepository.UpdateLoginHistoryAsync(existing, cancellationToken);
        logger.LogInformation("LoginHistory logged out: {Id}", command.Id.Value);
    }
}
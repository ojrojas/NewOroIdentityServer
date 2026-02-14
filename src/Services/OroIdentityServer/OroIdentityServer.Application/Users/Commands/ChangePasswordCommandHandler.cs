namespace OroIdentityServer.Application.Commands;

public class ChangePasswordCommandHandler(
    ILogger<ChangePasswordCommandHandler> logger,
    IUserRepository userRepository)
    : ICommandHandler<ChangePasswordCommand>
{
    public async Task HandleAsync(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling ChangePasswordCommand for {Email}", command.Email);

        var changed = await userRepository.ChangePasswordAsync(
            command.Email,
            command.CurrentPassword,
            command.NewPassword,
            command.ConfirmedPassword,
            cancellationToken);

        if (!changed)
        {
            logger.LogWarning("Change password failed for {Email}", command.Email);
            throw new InvalidOperationException("Unable to change password.");
        }

        logger.LogInformation("Password changed successfully for {Email}", command.Email);
    }
}
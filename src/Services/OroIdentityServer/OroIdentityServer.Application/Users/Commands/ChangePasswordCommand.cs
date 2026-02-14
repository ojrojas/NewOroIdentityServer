namespace OroIdentityServer.Application.Commands;

public record ChangePasswordCommand(
    string Email,
    string CurrentPassword,
    string NewPassword,
    string ConfirmedPassword
) : ICommand
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}
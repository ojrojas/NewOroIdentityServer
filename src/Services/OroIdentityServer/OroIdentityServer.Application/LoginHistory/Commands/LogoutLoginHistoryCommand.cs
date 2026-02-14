namespace OroIdentityServer.Application.Commands;

public record LogoutLoginHistoryCommand(LoginHistoryId Id) : ICommand
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}
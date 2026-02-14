namespace OroIdentityServer.Application.Commands;

public record DeleteUserRoleCommand(UserId UserId, RoleId RoleId) : ICommand
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}
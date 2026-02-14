namespace OroIdentityServer.Application.Commands;

public record AddUserRoleCommand(UserId UserId, RoleId RoleId) : ICommand
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}
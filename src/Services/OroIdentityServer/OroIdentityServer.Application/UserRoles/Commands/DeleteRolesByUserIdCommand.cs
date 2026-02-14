namespace OroIdentityServer.Application.Commands;

public record DeleteRolesByUserIdCommand(UserId UserId) : ICommand
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}
namespace OroIdentityServer.Application.Commands;

public record UpdateAuthorizationCommand(
    Guid Id,
    string? NewStatus = null,
    string? NewProperties = null
) : ICommand
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}
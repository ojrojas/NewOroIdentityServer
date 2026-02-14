namespace OroIdentityServer.Application.Commands;

public record UpdateUserSessionCommand(
    string RefreshToken,
    bool? Deactivate = null
) : ICommand
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}
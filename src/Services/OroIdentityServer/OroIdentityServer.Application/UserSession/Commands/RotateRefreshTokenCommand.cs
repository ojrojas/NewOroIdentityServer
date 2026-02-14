namespace OroIdentityServer.Application.Commands;

public record RotateRefreshTokenCommand(
    string OldRefreshToken,
    string NewRefreshToken
) : ICommand
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}

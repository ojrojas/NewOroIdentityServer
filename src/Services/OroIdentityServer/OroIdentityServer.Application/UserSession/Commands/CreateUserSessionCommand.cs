namespace OroIdentityServer.Application.Commands;

public record CreateUserSessionCommand(
    UserId UserId,
    string RefreshToken,
    string IpAddress,
    string? Country,
    DateTime LoginTime,
    DateTime ExpiresAt
) : ICommand
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}
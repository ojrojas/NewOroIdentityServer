namespace OroIdentityServer.Application.Commands;

public record CreateRevocationCommand(string Jti, DateTime RevokedAt, DateTime? ExpiresAt) : ICommand
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}

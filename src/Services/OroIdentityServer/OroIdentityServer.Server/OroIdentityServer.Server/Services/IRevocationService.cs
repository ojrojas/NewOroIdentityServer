namespace OroIdentityServer.Server.Services;

public interface IRevocationService
{
    Task Revoke(string jti, DateTime? expiresAt = null, CancellationToken cancellationToken = default);
    Task<bool> IsRevoked(string jti, CancellationToken cancellationToken = default);
}

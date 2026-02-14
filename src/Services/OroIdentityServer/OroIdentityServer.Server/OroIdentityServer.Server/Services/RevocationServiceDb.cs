using OroCQRS.Core.Interfaces;

namespace OroIdentityServer.Server.Services;

public class RevocationServiceDb(ISender sender) : IRevocationService
{
    private readonly ISender _sender = sender;

    public async Task Revoke(string jti, DateTime? expiresAt = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(jti)) return;
        await _sender.Send(new Application.Commands.CreateRevocationCommand(jti, DateTime.UtcNow, expiresAt), cancellationToken);
    }

    public async Task<bool> IsRevoked(string jti, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(jti)) return false;
        return await _sender.Send(new Application.Queries.IsRevokedQuery(jti), cancellationToken);
    }
}

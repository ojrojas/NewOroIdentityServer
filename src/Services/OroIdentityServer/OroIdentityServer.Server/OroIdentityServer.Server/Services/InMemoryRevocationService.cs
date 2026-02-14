using System.Collections.Concurrent;

namespace OroIdentityServer.Server.Services;

public class InMemoryRevocationService : IRevocationService
{
    private readonly ConcurrentDictionary<string, DateTime?> _store = new();

    public async Task Revoke(string jti, DateTime? expiresAt = null, CancellationToken cancellationToken = default)
    {
        if(cancellationToken.IsCancellationRequested) return;
        
        if (string.IsNullOrEmpty(jti)) return;
        _store[jti] = expiresAt;
    }
    public bool IsRevoked(string jti, CancellationToken cancellationToken)
    {
        if(cancellationToken.IsCancellationRequested || string.IsNullOrEmpty(jti)) return false;

        if (_store.TryGetValue(jti, out var exp))
        {
            // Null expiration means revoked indefinitely
            if (exp == null) return true;
            // If expiration is in the future, still revoked
            if (exp > DateTime.UtcNow) return true;
            // expired revocation entry: remove and report not revoked
            _store.TryRemove(jti, out _);
            return false;
        }
        return false;
    }

    Task IRevocationService.Revoke(string jti, DateTime? expiresAt, CancellationToken cancellationToken)
    {
        return Revoke(jti, expiresAt, cancellationToken);
    }

    Task<bool> IRevocationService.IsRevoked(string jti, CancellationToken cancellationToken)
    {
        var result = IsRevoked(jti, cancellationToken);
        return Task.FromResult(result);
    }
}

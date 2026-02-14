namespace OroIdentityServer.Core.Models;

public sealed class RevokedJti : BaseEntity<RevokedJti, RevokedJtiId>, IAuditableEntity, IAggregateRoot
{
    public string Jti { get; private set; }
    public DateTime RevokedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    private RevokedJti() { }

    public RevokedJti(RevokedJtiId id, string jti, DateTime revokedAt, DateTime? expiresAt)
    {
        Id = id;
        Jti = jti;
        RevokedAt = revokedAt;
        ExpiresAt = expiresAt;
        RaiseDomainEvent(new RevoketJtiCreatedDomainEvent(Id));
    }
}

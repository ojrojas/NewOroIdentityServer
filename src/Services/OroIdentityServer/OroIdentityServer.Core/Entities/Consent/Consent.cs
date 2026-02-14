namespace OroIdentityServer.Core.Models;

public sealed class Consent : BaseEntity<Consent, ConsentId>, IAuditableEntity, IAggregateRoot
{
    public UserId UserId { get; private set; }
    public ApplicationId ApplicationId { get; private set; }
    public string Scopes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool Remember { get; private set; }

    private Consent() { }

    public Consent(ConsentId id, UserId userId, ApplicationId applicationId, string scopes, DateTime createdAt, bool remember)
    {
        Id = id;
        UserId = userId;
        ApplicationId = applicationId;
        Scopes = scopes;
        CreatedAt = createdAt;
        Remember = remember;

        RaiseDomainEvent(new ConsentCreateEvent(Id, UserId, ApplicationId));
    }
}

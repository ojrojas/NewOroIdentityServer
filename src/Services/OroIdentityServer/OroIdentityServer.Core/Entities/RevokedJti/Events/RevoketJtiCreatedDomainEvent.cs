namespace OroIdentityServer.Core.Models;

public record RevoketJtiCreatedDomainEvent(RevokedJtiId RevokedJtiId) : DomainEventBase;

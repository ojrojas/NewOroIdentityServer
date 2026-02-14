namespace OroIdentityServer.Core.Models;

public record ConsentCreateEvent(ConsentId ConsentId, UserId UserId, ApplicationId ApplicationId) : DomainEventBase;

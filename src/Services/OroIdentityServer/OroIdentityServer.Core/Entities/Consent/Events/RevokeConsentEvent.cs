namespace OroIdentityServer.Core.Models;

public record RevokeConsentEvent(ConsentId ConsentId, UserId UserId, ApplicationId ApplicationId) : DomainEventBase;

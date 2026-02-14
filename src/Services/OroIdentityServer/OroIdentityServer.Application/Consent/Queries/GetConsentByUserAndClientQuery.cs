namespace OroIdentityServer.Application.Queries;

public record GetConsentByUserAndClientQuery(UserId UserId, Core.Models.ApplicationId ApplicationId) : IQuery<Consent?>
{
    public Guid CorrelationId() => Guid.NewGuid();
}
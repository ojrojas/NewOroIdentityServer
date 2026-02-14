namespace OroIdentityServer.Infraestructure.Interfaces;

public interface IUserConsentRepository
{
    Task AddConsentAsync(Consent consent, CancellationToken cancellationToken);
    Task<Consent?> GetConsentByUserAndClientAsync(UserId userId, Core.Models.ApplicationId applicationId, CancellationToken cancellationToken);
}

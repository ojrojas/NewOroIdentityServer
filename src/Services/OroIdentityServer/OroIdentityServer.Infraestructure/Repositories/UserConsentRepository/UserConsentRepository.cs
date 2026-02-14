namespace OroIdentityServer.Infraestructure.Repositories;

public class UserConsentRepository(
    ILogger<UserConsentRepository> logger,
    IRepository<Consent> repository) : IUserConsentRepository
{
    public async Task AddConsentAsync(Consent consent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding consent for user {UserId} and app {AppId}", consent.UserId.Value, consent.ApplicationId.Value);
        await repository.AddAsync(consent, cancellationToken);
    }

    public async Task<Consent?> GetConsentByUserAndClientAsync(UserId userId, Core.Models.ApplicationId applicationId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving consent for user {UserId} and app {AppId}", userId.Value, applicationId.Value);
        return await repository.FindSingleAsync(c => c.UserId == userId && c.ApplicationId == applicationId, cancellationToken);
    }
}

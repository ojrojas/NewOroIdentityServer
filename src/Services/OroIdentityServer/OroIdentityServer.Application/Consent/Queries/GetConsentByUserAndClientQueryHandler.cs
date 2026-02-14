namespace OroIdentityServer.Application.Queries;

public class GetConsentByUserAndClientQueryHandler(
    ILogger<GetConsentByUserAndClientQueryHandler> logger,
    IUserConsentRepository repository)
    : IQueryHandler<GetConsentByUserAndClientQuery, Consent?>
{
    public async Task<Consent?> HandleAsync(GetConsentByUserAndClientQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetConsentByUserAndClientQuery for user {UserId} app {AppId}", query.UserId.Value, query.ApplicationId.Value);
        return await repository.GetConsentByUserAndClientAsync(query.UserId, query.ApplicationId, cancellationToken);
    }
}

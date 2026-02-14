namespace OroIdentityServer.Application.Queries;

public class GetUserSessionByRefreshTokenQueryHandler(
    ILogger<GetUserSessionByRefreshTokenQueryHandler> logger,
    IUserSessionRepository repository)
    : IQueryHandler<GetUserSessionByRefreshTokenQuery, GetUserSessionByRefreshTokenResponse>
{
    public async Task<GetUserSessionByRefreshTokenResponse> HandleAsync(GetUserSessionByRefreshTokenQuery query, CancellationToken cancellationToken)
    {
        var response = new GetUserSessionByRefreshTokenResponse();
        logger.LogInformation("Handling GetUserSessionByRefreshTokenQuery");
        response.Data = await repository.GetUserSessionByRefreshTokenAsync(query.RefreshToken, cancellationToken);
        logger.LogInformation("Handled GetUserSessionByRefreshTokenQuery successfully");
        return response;
    }
}
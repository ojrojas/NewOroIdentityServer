namespace OroIdentityServer.Application.Queries;

public record GetUserSessionByRefreshTokenQuery(string RefreshToken) : IQuery<GetUserSessionByRefreshTokenResponse>
{
    Guid IBaseMessage.CorrelationId() => Guid.NewGuid();
}
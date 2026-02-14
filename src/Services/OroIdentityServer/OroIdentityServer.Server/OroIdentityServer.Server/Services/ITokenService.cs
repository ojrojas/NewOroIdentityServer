namespace OroIdentityServer.Server.Services;

public interface ITokenService
{
    Task<string> CreateAuthorizationCodeAsync(Guid applicationId, string subject, string scopes, DateTime expiresAt, string? codeChallenge = null, string? codeChallengeMethod = null, string? nonce = null, CancellationToken cancellationToken = default);

    Task<(string accessToken, string? refreshToken, string? idToken)> ExchangeAuthorizationCodeAsync(string code, string? codeVerifier, Core.Models.Application application, Core.Models.UserId? userId, CancellationToken cancellationToken = default);

    Task<string> CreateAccessTokenJwtAsync(Core.Models.UserId? userId, Core.Models.Application application, string scopes, TimeSpan lifetime);

    Task<string> CreateRefreshTokenAsync(Core.Models.UserId? userId, Core.Models.Application application, DateTime expiresAt, CancellationToken cancellationToken = default);

    Task<(string accessToken, string refreshToken)> ExchangeRefreshTokenAsync(string refreshToken, Core.Models.Application application, CancellationToken cancellationToken = default);

    Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<IntrospectionResult> IntrospectAsync(string token, CancellationToken cancellationToken = default);

    Microsoft.IdentityModel.Tokens.JsonWebKeySet GetJwks();
    
    // Rotate the active signing key and add the public key to JWKS
    void RotateSigningKey();
}
using Microsoft.AspNetCore.Mvc;

namespace OroIdentityServer.Server.Endpoints;

public record AuthorizeRequest(Guid ApplicationId, string Subject, string? Scope, string? State, string? CodeChallenge, string? CodeChallengeMethod, string? Nonce, string? RedirectUri, bool Remember);

public record TokenRequest([property: FromForm(Name = "grant_type")] string GrantType,
    [property: FromForm(Name = "code")] string? Code,
    [property: FromForm(Name = "code_verifier")] string? CodeVerifier,
    [property: FromForm(Name = "client_id")] Guid? ClientId,
    [property: FromForm(Name = "client_secret")] string? ClientSecret,
    [property: FromForm(Name = "scope")] string? Scope,
    [property: FromForm(Name = "refresh_token")] string? RefreshToken
);

public record TokenResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public int ExpiresIn { get; init; }
    public string? RefreshToken { get; init; }
    public string? IdToken { get; init; }
}

public record RevokeRequest([property: FromForm(Name = "token")] string Token, [property: FromForm(Name = "token_type_hint")] string? TokenTypeHint);
public record IntrospectRequest([property: FromForm(Name = "token")] string Token);
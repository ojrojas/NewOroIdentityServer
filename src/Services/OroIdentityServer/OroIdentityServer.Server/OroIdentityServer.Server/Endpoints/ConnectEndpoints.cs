using System.Text;
using Microsoft.AspNetCore.Mvc;
using OroIdentityServer.Server.Services;
using OroIdentityServer.Infraestructure.Interfaces;
using OroCQRS.Core.Interfaces;

namespace OroIdentityServer.Server.Endpoints;

public static partial class ConnectEndpoints
{
    public static WebApplication MapConnectEndpoints(this WebApplication app)
    {

        // Developer helper: create an authorization code for a subject (used for non-interactive/testing)
        // Also supports redirect flows when `RedirectUri` is provided.
        app.MapPost("/connect/authorize",
            async ([FromForm] AuthorizeRequest req, HttpRequest httpRequest, ITokenService tokenService, IApplicationRepository applicationRepository, ISender sender, CancellationToken cancellationToken) =>
            {
                var appModel = await applicationRepository.GetByIdAsync(new Core.Models.ApplicationId(req.ApplicationId), cancellationToken);
                if (appModel == null) return Results.BadRequest(new { error = "invalid_client" });

                // Validate redirect_uri if provided
                if (!string.IsNullOrEmpty(req.RedirectUri))
                {
                    var isValid = appModel.RedirectUris?.Any(r => string.Equals(r, req.RedirectUri, StringComparison.OrdinalIgnoreCase)) ?? false;
                    if (!isValid) return Results.BadRequest(new { error = "invalid_request", error_description = "redirect_uri_mismatch" });
                }

                // If Subject not provided, try use authenticated user
                Core.Models.User? user = null;
                if (string.IsNullOrEmpty(req.Subject) && httpRequest.HttpContext.User?.Identity?.IsAuthenticated == true)
                {
                    var email = httpRequest.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? httpRequest.HttpContext.User.Identity.Name;
                    if (!string.IsNullOrEmpty(email))
                    {
                        try { var userResp = await sender.Send(new Application.Queries.GetUserByEmailQuery(email), cancellationToken); user = userResp?.Data; } catch { }
                    }
                }

                // If a subject/email was provided, try to resolve to a user in the system
                if (user == null && !string.IsNullOrEmpty(req.Subject))
                {
                    try
                    {
                        var userResp = await sender.Send(new Application.Queries.GetUserByEmailQuery(req.Subject), cancellationToken);
                        user = userResp?.Data;
                    }
                    catch { }
                }

                if (user == null)
                {
                    // For interactive login we require an existing user; reject otherwise
                    return Results.BadRequest(new { error = "invalid_request", error_description = "user_not_found" });
                }

                // Persist consent if user approved and chose to remember
                if (req.Remember)
                {
                    await sender.Send(new Application.Commands.CreateConsentCommand(new Core.Models.UserId(user.Id.Value), new Core.Models.ApplicationId(req.ApplicationId), req.Scope ?? string.Empty, true), cancellationToken);
                }

                var code = await tokenService.CreateAuthorizationCodeAsync(req.ApplicationId, req.Subject, req.Scope ?? string.Empty, DateTime.UtcNow.AddMinutes(5), req.CodeChallenge, req.CodeChallengeMethod, req.Nonce, cancellationToken);

                if (!string.IsNullOrEmpty(req.RedirectUri))
                {
                    var uri = req.RedirectUri;
                    var sep = uri.Contains('?') ? '&' : '?';
                    var dest = uri + sep + "code=" + Uri.EscapeDataString(code);
                    if (!string.IsNullOrEmpty(req.State)) dest += "&state=" + Uri.EscapeDataString(req.State);
                    return Results.Redirect(dest);
                }

                return Results.Ok(new { code, state = req.State });
            })
        .WithMetadata(new Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute())
        .WithName("Authorize")
        .Accepts<AuthorizeRequest>("application/x-www-form-urlencoded");

        // UI entry: redirect browser to consent page for interactive authorization
        app.MapGet("/connect/authorize", (HttpRequest httpRequest) =>
        {
            // forward the query to the consent page where the user can login/approve
            var query = httpRequest.QueryString.Value ?? string.Empty;
            return Results.Redirect("/Consent" + query);
        }).WithName("AuthorizeUi");

        app.MapPost("/connect/token",
            async ([FromForm] TokenRequest req, HttpRequest httpRequest, ITokenService tokenService, IApplicationRepository applicationRepository, CancellationToken cancellationToken) =>
            {
                // parse HTTP Basic auth if present
                string? basicClientId = null;
                string? basicSecret = null;
                if (httpRequest.Headers.TryGetValue("Authorization", out var authValues))
                {
                    var auth = authValues.ToString();
                    if (auth.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var payload = Encoding.UTF8.GetString(Convert.FromBase64String(auth.Substring(6)));
                            var idx = payload.IndexOf(':');
                            if (idx >= 0)
                            {
                                basicClientId = payload.Substring(0, idx);
                                basicSecret = payload.Substring(idx + 1);
                            }
                        }
                        catch { }
                    }
                }
                if (req.GrantType == "authorization_code")
                {
                    if (string.IsNullOrEmpty(req.Code) || req.ClientId == null) return Results.BadRequest(new { error = "invalid_request" });
                    // determine client id from form or Basic auth
                    var clientGuid = req.ClientId.Value;
                    if (!string.IsNullOrEmpty(basicClientId) && Guid.TryParse(basicClientId, out var parsedGuid)) clientGuid = parsedGuid;

                    var appModel = await applicationRepository.GetByIdAsync(new Core.Models.ApplicationId(clientGuid), cancellationToken);
                    if (appModel == null) return Results.BadRequest(new { error = "invalid_client" });

                    // if secret provided, validate it; otherwise require PKCE (code_verifier)
                    var presentedSecret = basicSecret ?? req.ClientSecret;
                    if (!string.IsNullOrEmpty(presentedSecret))
                    {
                        if (!appModel.ValidateClientSecret(presentedSecret)) return Results.BadRequest(new { error = "invalid_client" });
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(req.CodeVerifier)) return Results.BadRequest(new { error = "invalid_client", error_description = "Public clients must use PKCE or authenticate" });
                    }

                    var (access, refresh, idToken) = await tokenService.ExchangeAuthorizationCodeAsync(req.Code, req.CodeVerifier, appModel, null, cancellationToken);
                    return Results.Ok(new TokenResponse { AccessToken = access, TokenType = "Bearer", ExpiresIn = 60 * 60, RefreshToken = refresh, IdToken = idToken });
                }

                if (req.GrantType == "refresh_token")
                {
                    if (string.IsNullOrEmpty(req.RefreshToken) || req.ClientId == null) return Results.BadRequest(new { error = "invalid_request" });
                    // client authentication required for refresh token grant
                    var clientGuid = req.ClientId.Value;
                    if (!string.IsNullOrEmpty(basicClientId) && Guid.TryParse(basicClientId, out var parsedGuid)) clientGuid = parsedGuid;

                    var appModel = await applicationRepository.GetByIdAsync(new Core.Models.ApplicationId(clientGuid), cancellationToken);
                    if (appModel == null) return Results.BadRequest(new { error = "invalid_client" });

                    var presentedSecret = basicSecret ?? req.ClientSecret;
                    if (string.IsNullOrEmpty(presentedSecret) || !appModel.ValidateClientSecret(presentedSecret))
                        return Results.BadRequest(new { error = "invalid_client" });

                    var (access, refresh) = await tokenService.ExchangeRefreshTokenAsync(req.RefreshToken!, appModel, cancellationToken);
                    return Results.Ok(new TokenResponse { AccessToken = access, TokenType = "Bearer", ExpiresIn = 60 * 60, RefreshToken = refresh });
                }

                if (req.GrantType == "client_credentials")
                {
                    if (!req.ClientId.HasValue && string.IsNullOrEmpty(basicClientId)) return Results.BadRequest(new { error = "invalid_client" });

                    Guid clientGuid;
                    if (!string.IsNullOrEmpty(basicClientId) && Guid.TryParse(basicClientId, out var parsed)) clientGuid = parsed;
                    else clientGuid = req.ClientId.Value;

                    var appModel = await applicationRepository.GetByIdAsync(new Core.Models.ApplicationId(clientGuid), cancellationToken);
                    if (appModel == null) return Results.BadRequest(new { error = "invalid_client" });

                    var presentedSecret = basicSecret ?? req.ClientSecret;
                    if (string.IsNullOrEmpty(presentedSecret) || !appModel.ValidateClientSecret(presentedSecret))
                        return Results.BadRequest(new { error = "invalid_client" });

                    var access = await tokenService.CreateAccessTokenJwtAsync(null, appModel, req.Scope ?? string.Empty, TimeSpan.FromMinutes(60));
                    return Results.Ok(new TokenResponse { AccessToken = access, TokenType = "Bearer", ExpiresIn = 60 * 60 });
                }

                return Results.BadRequest(new { error = "unsupported_grant_type" });
            })
        .WithMetadata(new Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute())
        .WithName("Token")
        .Accepts<TokenRequest>("application/x-www-form-urlencoded");

        app.MapPost("/connect/revoke",
            async ([FromForm] RevokeRequest req, HttpRequest httpRequest, ITokenService tokenService, IApplicationRepository applicationRepository, CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrEmpty(req.Token)) return Results.Ok();

                // parse HTTP Basic auth if present
                string? basicClientId = null;
                string? basicSecret = null;
                if (httpRequest.Headers.TryGetValue("Authorization", out var authValues))
                {
                    var auth = authValues.ToString();
                    if (auth.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var payload = Encoding.UTF8.GetString(Convert.FromBase64String(auth.Substring(6)));
                            var idx = payload.IndexOf(':');
                            if (idx >= 0)
                            {
                                basicClientId = payload.Substring(0, idx);
                                basicSecret = payload.Substring(idx + 1);
                            }
                        }
                        catch { }
                    }
                }

                // if token_type_hint is refresh_token, require client authentication
                if (string.Equals(req.TokenTypeHint, "refresh_token", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(basicClientId) && string.IsNullOrEmpty(httpRequest.Form["client_id"]))
                        return Results.Unauthorized();

                    Guid clientGuid = Guid.Empty;
                    if (!string.IsNullOrEmpty(basicClientId) && Guid.TryParse(basicClientId, out var parsed)) clientGuid = parsed;
                    else if (Guid.TryParse(httpRequest.Form["client_id"], out var parsed2)) clientGuid = parsed2;

                    if (clientGuid == Guid.Empty) return Results.Unauthorized();

                    var appModel = await applicationRepository.GetByIdAsync(new Core.Models.ApplicationId(clientGuid), cancellationToken);
                    if (appModel == null) return Results.Unauthorized();

                    var presentedSecret = basicSecret ?? httpRequest.Form["client_secret"].ToString();
                    if (string.IsNullOrEmpty(presentedSecret) || !appModel.ValidateClientSecret(presentedSecret))
                        return Results.Unauthorized();
                }

                await tokenService.RevokeTokenAsync(req.Token, cancellationToken);
                return Results.Ok();
            })
        .WithMetadata(new Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute())
        .WithName("Revoke")
        .Accepts<RevokeRequest>("application/x-www-form-urlencoded");

        app.MapPost("/connect/logout",
            async ([FromForm] LogoutRequest req, ISender sender, CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrEmpty(req.RefreshToken)) return Results.BadRequest(new { error = "invalid_request" });

                // Deactivate the user session linked to this refresh token
                await sender.Send(new Application.Commands.UpdateUserSessionCommand(req.RefreshToken, true), cancellationToken);

                if (!string.IsNullOrEmpty(req.PostLogoutRedirectUri))
                    return Results.Redirect(req.PostLogoutRedirectUri);

                return Results.Ok();
            })
        .WithMetadata(new Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute())
        .WithName("Logout")
        .Accepts<LogoutRequest>("application/x-www-form-urlencoded");

        // Back-channel logout endpoint: accept JSON payload with jti or refresh_token
        app.MapPost("/connect/backchannel_logout",
            async ([FromBody] BackchannelLogoutRequest req, IRevocationService revocationService, ISender sender, CancellationToken cancellationToken) =>
            {
                if (req is null) return Results.BadRequest(new { error = "invalid_request" });

                if (!string.IsNullOrEmpty(req.Jti))
                {
                    await revocationService.Revoke(req.Jti, null, cancellationToken);
                }

                if (!string.IsNullOrEmpty(req.RefreshToken))
                {
                    await sender.Send(new Application.Commands.UpdateUserSessionCommand(req.RefreshToken, true), cancellationToken);
                }

                return Results.Ok();
            })
        .WithName("BackchannelLogout")
        .Accepts<BackchannelLogoutRequest>("application/json");

        app.MapPost("/connect/introspect",
            async ([FromForm] IntrospectRequest req, ITokenService tokenService, CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrEmpty(req.Token)) return Results.BadRequest();
                var r = await tokenService.IntrospectAsync(req.Token, cancellationToken);

                // Build RFC 7662 response
                var resp = new Dictionary<string, object?>();
                resp["active"] = r.Active;
                if (!r.Active) return Results.Ok(resp);

                if (!string.IsNullOrEmpty(r.Scope)) resp["scope"] = r.Scope;
                if (!string.IsNullOrEmpty(r.ClientId)) resp["client_id"] = r.ClientId;
                if (!string.IsNullOrEmpty(r.Sub)) resp["username"] = r.Sub; // username or sub
                if (!string.IsNullOrEmpty(r.TokenType)) resp["token_type"] = r.TokenType;
                if (r.Exp.HasValue) resp["exp"] = r.Exp.Value;
                if (r.Iat.HasValue) resp["iat"] = r.Iat.Value;
                if (r.Nbf.HasValue) resp["nbf"] = r.Nbf.Value;
                if (!string.IsNullOrEmpty(r.Sub)) resp["sub"] = r.Sub;
                if (!string.IsNullOrEmpty(r.Aud)) resp["aud"] = r.Aud;
                if (!string.IsNullOrEmpty(r.Iss)) resp["iss"] = r.Iss;
                if (!string.IsNullOrEmpty(r.Jti)) resp["jti"] = r.Jti;

                return Results.Ok(resp);
            })
        .WithMetadata(new Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute())
        .WithName("Introspect")
        .Accepts<IntrospectRequest>("application/x-www-form-urlencoded");

        return app;
    }
}

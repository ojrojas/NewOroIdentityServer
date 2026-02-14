using OroIdentityServer.Server.Services;

namespace OroIdentityServer.Server.Endpoints;

public static class OpenIdEndpoints
{
    public static WebApplication MapOpenIdEndpoints(this WebApplication app)
    {
        app.MapGet("/.well-known/openid-configuration", (HttpRequest req, ITokenService tokenService) =>
        {
            var issuer = req.Scheme + "://" + req.Host.Value;
            var baseUrl = issuer.TrimEnd('/');
            var discovery = new Dictionary<string, object?>
            {
                ["issuer"] = baseUrl,
                ["authorization_endpoint"] = baseUrl + "/connect/authorize",
                ["token_endpoint"] = baseUrl + "/connect/token",
                ["jwks_uri"] = baseUrl + "/.well-known/jwks.json",
                ["response_types_supported"] = new[] { "code" },
                ["subject_types_supported"] = new[] { "public" },
                ["id_token_signing_alg_values_supported"] = new[] { "RS256", "HS256" }
            };

            return Results.Json(discovery);
        }).WithName("OpenIdConfiguration");

        app.MapGet("/.well-known/jwks.json", (HttpRequest req, ITokenService tokenService) =>
        {
            var jwks = tokenService.GetJwks();
            return Results.Json(new { keys = jwks.Keys });
        }).WithName("Jwks");

        // Admin endpoint to rotate signing key (adds a new key to JWKS and makes it active).
        app.MapPost("/admin/rotate-key", (ITokenService tokenService) =>
        {
            if (tokenService is TokenService ts)
            {
                ts.RotateSigningKey();
                return Results.Ok(new { rotated = true });
            }
            return Results.BadRequest(new { rotated = false, reason = "unsupported_service" });
        }).WithName("RotateKey");

        app.MapGet("/connect/userinfo", async (HttpRequest req, ITokenService tokenService) =>
        {
            // Expect Authorization: Bearer <token>
            if (!req.Headers.TryGetValue("Authorization", out var authValues)) return Results.Unauthorized();
            var auth = authValues.ToString();
            if (!auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return Results.Unauthorized();
            var token = auth.Substring(7).Trim();
            if (string.IsNullOrEmpty(token)) return Results.Unauthorized();

            var info = await tokenService.IntrospectAsync(token);
            if (!info.Active) return Results.Unauthorized();

            var profile = new Dictionary<string, object?>
            {
                ["sub"] = info.Sub,
                ["scope"] = info.Scope,
                ["client_id"] = info.ClientId
            };
            return Results.Json(profile);
        }).WithName("UserInfo");

        return app;
    }
}

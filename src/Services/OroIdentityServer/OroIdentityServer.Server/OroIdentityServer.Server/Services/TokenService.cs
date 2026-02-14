using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OroCQRS.Core.Interfaces;
using System.Text.Json;
using System.Security.Cryptography;
using OroIdentityServer.Application.Queries;

namespace OroIdentityServer.Server.Services;

public class TokenServiceOptions
{
    public string Issuer { get; set; } = "http://localhost";
    public string Audience { get; set; } = "api";
    // For symmetric HMAC (HS256) set SigningKey. For RSA set SigningAlgorithm = "RS256" and optionally provide SigningKeyPrivatePem.
    public string SigningKey { get; set; } = "dev-signing-key-change-me-for-prod";
    public string SigningAlgorithm { get; set; } = "RS256";
    public string? SigningKeyPrivatePem { get; set; }
    public string? SigningKeyKid { get; set; }
    public int KeyRetentionDays { get; set; } = 7;
    public string SigningKeysDirectory { get; set; } = "signing_keys";
    public bool EnforcePkcePlain { get; set; } = true;
    public int AccessTokenLifetimeMinutes { get; set; } = 60;
    public int RefreshTokenLifetimeDays { get; set; } = 30;
    public bool UseJwtAccessTokens { get; set; } = true;
}

public class TokenService : ITokenService
{
    private readonly TokenServiceOptions _opts;
    private readonly ISender _sender;
    private readonly ILogger<TokenService> _logger;
    private SigningCredentials _signingCredentials;
    private readonly JsonWebKeySet _jwks;
    private readonly IRevocationService _revocationService;

    public TokenService(IOptions<TokenServiceOptions> opts, ISender sender, ILogger<TokenService> logger, IRevocationService revocationService)
    {
        _opts = opts.Value;
        _sender = sender;
        _logger = logger;
        _revocationService = revocationService;

        var signingAlg = _opts.SigningAlgorithm ?? "RS256";

        // Setup signing credentials and JWKS. Prefer asymmetric RSA when SigningAlgorithm starts with "RS"
        if (signingAlg.StartsWith("RS", StringComparison.OrdinalIgnoreCase))
        {
            // Prepare JWKS and signing cred holder
            _jwks = new JsonWebKeySet();

            // Ensure directory exists for persisted keys
            var keysDir = Path.Combine(Directory.GetCurrentDirectory(), _opts.SigningKeysDirectory ?? "signing_keys");
            Directory.CreateDirectory(keysDir);

            // Load persisted PEM files (private keys) and add their public JWKs
            var pemFiles = Directory.GetFiles(keysDir, "*.pem");
            string? latestKeyId = null;
            DateTime latestWrite = DateTime.MinValue;

            foreach (var pemFile in pemFiles)
            {
                try
                {
                    var pem = File.ReadAllText(pemFile);
                    var rsa = RSA.Create();
                    rsa.ImportFromPem(pem.ToCharArray());
                    var rsaKey = new RsaSecurityKey(rsa) { KeyId = Path.GetFileNameWithoutExtension(pemFile) };
                    var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(rsaKey);
                    jwk.Kid = rsaKey.KeyId;
                    jwk.Use = "sig";
                    jwk.Alg = SecurityAlgorithms.RsaSha256;
                    _jwks.Keys.Add(jwk);

                    var fi = new FileInfo(pemFile);
                    if (fi.LastWriteTimeUtc > latestWrite)
                    {
                        latestWrite = fi.LastWriteTimeUtc;
                        latestKeyId = rsaKey.KeyId;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load persisted signing key {file}", pemFile);
                }
            }

            // If SigningKeyPrivatePem provided via config, use it and persist
            if (!string.IsNullOrEmpty(_opts.SigningKeyPrivatePem))
            {
                try
                {
                    var rsa = RSA.Create();
                    rsa.ImportFromPem(_opts.SigningKeyPrivatePem.ToCharArray());
                    var kid = !string.IsNullOrEmpty(_opts.SigningKeyKid) ? _opts.SigningKeyKid : Guid.NewGuid().ToString("N");
                    var rsaKey = new RsaSecurityKey(rsa) { KeyId = kid };
                    _signingCredentials = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256);

                    var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(rsaKey);
                    jwk.Kid = rsaKey.KeyId;
                    jwk.Use = "sig";
                    jwk.Alg = SecurityAlgorithms.RsaSha256;
                    // ensure added
                    if (!_jwks.Keys.Any(k => k.Kid == jwk.Kid)) _jwks.Keys.Add(jwk);

                    // Persist private PEM file if not exists
                    var pemPath = Path.Combine(keysDir, kid + ".pem");
                    if (!File.Exists(pemPath))
                    {
                        var priv = ExportPrivateKeyPem(rsa);
                        File.WriteAllText(pemPath, priv);
                    }

                    latestKeyId = kid;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to import SigningKeyPrivatePem, generating ephemeral RSA key");
                }
            }

            // If no signing credentials yet, but persisted keys exist, pick latest persisted as active
            if (_signingCredentials == null && latestKeyId != null)
            {
                // Load RSA from file
                var topPem = Path.Combine(keysDir, latestKeyId + ".pem");
                try
                {
                    var pem = File.ReadAllText(topPem);
                    var rsa = RSA.Create();
                    rsa.ImportFromPem(pem.ToCharArray());
                    var rsaKey = new RsaSecurityKey(rsa) { KeyId = latestKeyId };
                    _signingCredentials = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to set signing credentials from persisted key {kid}", latestKeyId);
                }
            }

            // If still none, generate ephemeral
            if (_signingCredentials == null)
            {
                var rsa = RSA.Create(2048);
                var rsaKey = new RsaSecurityKey(rsa) { KeyId = !string.IsNullOrEmpty(_opts.SigningKeyKid) ? _opts.SigningKeyKid : Guid.NewGuid().ToString("N") };
                _signingCredentials = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256);
                var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(rsaKey);
                jwk.Kid = rsaKey.KeyId;
                jwk.Use = "sig";
                jwk.Alg = SecurityAlgorithms.RsaSha256;
                _jwks.Keys.Add(jwk);
            }
        }
        else
        {
            // Symmetric fallback (HMAC)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.SigningKey));
            key.KeyId = Guid.NewGuid().ToString("N");
            _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // symmetric key not published in JWKS
            _jwks = new JsonWebKeySet();
        }
    }

    // Rotates the signing key: generates a new RSA key and makes it the active signing key while
    // adding the public part to the JWKS. This allows multiple keys to be published for rotation.
    public void RotateSigningKey()
    {
        // Only support RSA rotation for now when algorithm is RS*
        var signingAlg = _opts.SigningAlgorithm ?? "RS256";
        if (!signingAlg.StartsWith("RS", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException("Key rotation only supported for RSA signing algorithm in this implementation.");

        var rsa = RSA.Create(2048);
        var kid = Guid.NewGuid().ToString("N");
        var rsaKey = new RsaSecurityKey(rsa) { KeyId = kid };
        var newCred = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256);

        // Add public JWK to set
        var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(rsaKey);
        jwk.Kid = rsaKey.KeyId;
        jwk.Use = "sig";
        jwk.Alg = SecurityAlgorithms.RsaSha256;
        _jwks.Keys.Add(jwk);

        // Persist private key PEM
        try
        {
            var keysDir = Path.Combine(Directory.GetCurrentDirectory(), _opts.SigningKeysDirectory ?? "signing_keys");
            Directory.CreateDirectory(keysDir);
            var pem = ExportPrivateKeyPem(rsa);
            var path = Path.Combine(keysDir, kid + ".pem");
            File.WriteAllText(path, pem);

            // Remove old private keys beyond retention
            try
            {
                var retention = TimeSpan.FromDays(_opts.KeyRetentionDays);
                var files = Directory.GetFiles(keysDir, "*.pem");
                foreach (var f in files)
                {
                    try
                    {
                        if (f == path) continue;
                        var fi = new FileInfo(f);
                        if (DateTime.UtcNow - fi.LastWriteTimeUtc > retention)
                        {
                            File.Delete(f);
                            _logger.LogInformation("Deleted old signing key file {file}", f);
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to prune old signing keys");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist rotated signing key");
        }

        // set new signing credentials
        _signingCredentials = newCred;
        _logger.LogInformation("Rotated signing key. New kid={kid}", rsaKey.KeyId);
    }

    public async Task<string> CreateAuthorizationCodeAsync(Guid applicationId, string subject, string scopes, DateTime expiresAt, string? codeChallenge = null, string? codeChallengeMethod = null, string? nonce = null, CancellationToken cancellationToken = default)
    {
        var app = await _sender.Send(new Application.Application.Queries.GetApplicationByIdQuery(new Core.Models.ApplicationId(applicationId)), cancellationToken);
        ArgumentNullException.ThrowIfNull(app, nameof(app));

        var code = Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

        var payload = new Dictionary<string, object?>
        {
            ["sub"] = subject,
            ["scopes"] = scopes,
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = codeChallengeMethod
        };

        if (!string.IsNullOrEmpty(nonce)) payload["nonce"] = nonce;

        // if scopes include openid, a nonce provided by client should be stored
        try
        {
            // attempt to read nonce from app request context via sender? fallback handled by caller to pass null
        }
        catch { }

        await _sender.Send(new Application.Token.Commands.CreateTokenCommand(
            app.Id!,
            "authorization_code",
            code,
            JsonSerializer.Serialize(payload),
            DateTime.UtcNow,
            expiresAt,
            "active",
            subject,
            null
        ), cancellationToken);

        _logger.LogDebug("Created authorization code for app {app} subject {sub}", applicationId, subject);
        return code;
    }

    public async Task<(string accessToken, string? refreshToken, string? idToken)> ExchangeAuthorizationCodeAsync(string code, string? codeVerifier, Core.Models.Application application, Core.Models.UserId? userId, CancellationToken cancellationToken = default)
    {
        // find authorization code token
        var stored = await _sender.Send(new GetTokenByReferenceQuery(code), cancellationToken);
        if (stored == null) throw new InvalidOperationException("Invalid or expired authorization code.");
        if (stored.ExpirationDate.HasValue && stored.ExpirationDate < DateTime.UtcNow) throw new InvalidOperationException("Authorization code expired.");

        // PKCE verification if present
        if (!string.IsNullOrEmpty(stored.Payload))
        {
            try
            {
                var doc = JsonDocument.Parse(stored.Payload!);
                if (doc.RootElement.TryGetProperty("code_challenge", out var ccEl) && ccEl.ValueKind == JsonValueKind.String)
                {
                    var expected = ccEl.GetString();
                    if (!string.IsNullOrEmpty(expected))
                    {
                        // only plain and S256 supported
                        var method = doc.RootElement.GetProperty("code_challenge_method").GetString() ?? "S256";
                        if (string.Equals(method, "S256", StringComparison.OrdinalIgnoreCase))
                        {
                            var hashed = Base64UrlEncode(Sha256(codeVerifier ?? string.Empty));
                            if (hashed != expected) throw new InvalidOperationException("Invalid PKCE verifier.");
                        }
                        else
                        {
                            // plain
                            if (_opts.EnforcePkcePlain)
                                throw new InvalidOperationException("Plain PKCE is not allowed by policy.");
                            if (codeVerifier != expected) throw new InvalidOperationException("Invalid PKCE verifier.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to validate PKCE payload");
            }
        }

        // mark code as redeemed
        await _sender.Send(new Application.Token.Commands.UpdateTokenStatusCommand(stored.ReferenceId, "redeemed", DateTime.UtcNow), cancellationToken);

        // issue access token (JWT by default) and refresh token
        string scopes = string.Empty;
        if (!string.IsNullOrEmpty(stored.Payload))
        {
            try
            {
                var doc = JsonDocument.Parse(stored.Payload);
                if (doc.RootElement.TryGetProperty("scopes", out var scEl) && scEl.ValueKind == JsonValueKind.String)
                    scopes = scEl.GetString() ?? string.Empty;
            }
            catch { /* ignore */ }
        }

        var accessToken = _opts.UseJwtAccessTokens
            ? await CreateAccessTokenJwtAsync(userId, application, scopes, TimeSpan.FromMinutes(_opts.AccessTokenLifetimeMinutes))
            : throw new NotSupportedException("Opaque access tokens not implemented yet.");

        var refreshToken = await CreateRefreshTokenAsync(userId, application, DateTime.UtcNow.AddDays(_opts.RefreshTokenLifetimeDays), cancellationToken);

        // create id_token if openid scope requested
        string? idToken = null;
        try
        {
            if (!string.IsNullOrEmpty(scopes) && scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries).Any(s => s == "openid"))
            {
                // extract nonce from stored payload if available
                string? nonce = null;
                if (!string.IsNullOrEmpty(stored.Payload))
                {
                    try
                    {
                        var doc = JsonDocument.Parse(stored.Payload);
                        if (doc.RootElement.TryGetProperty("nonce", out var n) && n.ValueKind == JsonValueKind.String)
                            nonce = n.GetString();
                    }
                    catch { }
                }

                idToken = await CreateIdTokenAsync(userId, application, nonce, DateTime.UtcNow);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create id_token");
        }

        return (accessToken, refreshToken, idToken);
    }

    public Task<string> CreateIdTokenAsync(Core.Models.UserId? userId, Core.Models.Application application, string? nonce, DateTime authTime)
    {
        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId?.Value.ToString() ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iss, _opts.Issuer),
            new Claim(JwtRegisteredClaimNames.Aud, application.Id?.Value.ToString() ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim("auth_time", new DateTimeOffset(authTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        if (!string.IsNullOrEmpty(nonce))
            claims.Add(new Claim("nonce", nonce));
        var token = new JwtSecurityToken(
            issuer: _opts.Issuer,
            audience: application.Id?.Value.ToString() ?? _opts.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(5), // id_token short-lived
            signingCredentials: _signingCredentials
        );

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        return Task.FromResult(tokenValue);
    }

    public async Task<(string accessToken, string refreshToken)> ExchangeRefreshTokenAsync(string refreshToken, Core.Models.Application application, CancellationToken cancellationToken = default)
    {
        var resp = await _sender.Send(new GetUserSessionByRefreshTokenQuery(refreshToken), cancellationToken);
        var session = resp?.Data;
        if (session == null) throw new InvalidOperationException("Invalid refresh token.");
        if (!session.IsActive || session.IsExpired()) throw new InvalidOperationException("Refresh token expired or inactive.");

        // rotate refresh token
        var newRefresh = Base64UrlEncode(RandomNumberGenerator.GetBytes(48));
        await _sender.Send(new Application.Commands.RotateRefreshTokenCommand(refreshToken, newRefresh), cancellationToken);

        var accessToken = _opts.UseJwtAccessTokens
            ? await CreateAccessTokenJwtAsync(session.UserId, application, string.Empty, TimeSpan.FromMinutes(_opts.AccessTokenLifetimeMinutes))
            : throw new NotSupportedException("Opaque access tokens not implemented yet.");

        return (accessToken, newRefresh);
    }

    public Task<string> CreateAccessTokenJwtAsync(Core.Models.UserId? userId, Core.Models.Application application, string scopes, TimeSpan lifetime)
    {
        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId?.Value.ToString() ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("scope", scopes),
            new Claim("client_id", application.Id?.Value.ToString() ?? string.Empty)
        };
        var token = new JwtSecurityToken(
            issuer: _opts.Issuer,
            audience: _opts.Audience,
            claims: claims,
            notBefore: now,
            expires: now.Add(lifetime),
            signingCredentials: _signingCredentials
        );

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        return Task.FromResult(tokenValue);
    }

    public async Task<string> CreateRefreshTokenAsync(Core.Models.UserId? userId, Core.Models.Application application, DateTime expiresAt, CancellationToken cancellationToken = default)
    {
        var refresh = Base64UrlEncode(RandomNumberGenerator.GetBytes(48));

        // create a user session via Application command
        await _sender.Send(new Application.Commands.CreateUserSessionCommand(userId ?? Core.Models.UserId.New(), refresh, "127.0.0.1", null, DateTime.UtcNow, expiresAt), cancellationToken);
        return refresh;
    }

    public async Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        // Try to find reference token (refresh or opaque) by ReferenceId
        var stored = await _sender.Send(new GetTokenByReferenceQuery(token), cancellationToken);
        if (stored != null)
        {
            await _sender.Send(new Application.Token.Commands.UpdateTokenStatusCommand(token, "revoked", DateTime.UtcNow), cancellationToken);
            return;
        }
        // If token is JWT, attempt to extract jti and record revocation
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var jti = jwt.Id;
            DateTime? exp = null;
            if (jwt.Payload.Exp.HasValue)
                exp = DateTimeOffset.FromUnixTimeSeconds(jwt.Payload.Exp.Value).UtcDateTime;
            if (!string.IsNullOrEmpty(jti))
            {
                _revocationService.Revoke(jti, exp);
                _logger.LogInformation("Revoked JWT jti={jti}", jti);
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to parse token during revoke");
        }

        _logger.LogInformation("Revoke requested for unknown token: {token}", token);
    }

    public async Task<IntrospectionResult> IntrospectAsync(string token, CancellationToken cancellationToken = default)
    {
        // Check DB for reference tokens first
        var stored = await _sender.Send(new GetTokenByReferenceQuery(token), cancellationToken);
        if (stored != null)
        {
            var active = stored.Status == "active" && (!stored.ExpirationDate.HasValue || stored.ExpirationDate > DateTime.UtcNow) && stored.RedemptionDate == null;
            var result = new IntrospectionResult
            {
                Active = active,
                ClientId = stored.Application?.Id?.Value.ToString(),
                Sub = stored.Subject,
                Exp = stored.ExpirationDate.HasValue ? new DateTimeOffset(stored.ExpirationDate.Value).ToUnixTimeSeconds() : null,
                Iat = stored.CreationDate.HasValue ? new DateTimeOffset(stored.CreationDate.Value).ToUnixTimeSeconds() : null,
                TokenType = stored.Type
            };
            return result;
        }

        // Attempt to validate as JWT
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _opts.Issuer,
                ValidateAudience = true,
                ValidAudience = _opts.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingCredentials.Key,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };

            var principal = handler.ValidateToken(token, parameters, out var validatedToken);
            if (principal == null) return new IntrospectionResult { Active = false };

            var jwt = validatedToken as JwtSecurityToken;
            var result = new IntrospectionResult
            {
                Active = true,
                Sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value,
                ClientId = principal.FindFirst("client_id")?.Value,
                Scope = principal.FindFirst("scope")?.Value,
                Iat = jwt?.Payload.Iat.HasValue == true ? jwt.Payload.Iat : null,
                Exp = jwt?.Payload.Exp.HasValue == true ? jwt.Payload.Exp : null,
                Nbf = jwt?.Payload.Nbf.HasValue == true ? jwt.Payload.Nbf : null,
                Iss = jwt?.Issuer,
                Aud = jwt?.Audiences?.FirstOrDefault(),
                Jti = jwt?.Id,
                TokenType = "access_token"
            };

            // check revocation
            if (!string.IsNullOrEmpty(result.Jti) && await _revocationService.IsRevoked(result.Jti, cancellationToken))
            {
                result.Active = false;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "JWT introspection failed");
            return new IntrospectionResult { Active = false };
        }
    }

    public JsonWebKeySet GetJwks() => _jwks;

    private static string ExportPrivateKeyPem(RSA rsa)
    {
        var pkcs8 = rsa.ExportPkcs8PrivateKey();
        var base64 = Convert.ToBase64String(pkcs8);
        var sb = new StringBuilder();
        sb.AppendLine("-----BEGIN PRIVATE KEY-----");
        for (int i = 0; i < base64.Length; i += 64)
            sb.AppendLine(base64.Substring(i, Math.Min(64, base64.Length - i)));
        sb.AppendLine("-----END PRIVATE KEY-----");
        return sb.ToString();
    }

    private static byte[] Sha256(string value)
    {
        if (value == null) return Array.Empty<byte>();
        return SHA256.HashData(Encoding.UTF8.GetBytes(value));
    }

    private static string Base64UrlEncode(byte[] bytes)
        => Base64UrlEncoder.Encode(bytes);
}

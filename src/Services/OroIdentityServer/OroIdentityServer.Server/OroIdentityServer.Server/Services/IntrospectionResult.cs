namespace OroIdentityServer.Server.Services;

public class IntrospectionResult
{
    public bool Active { get; set; }
    public string? Scope { get; set; }
    public string? ClientId { get; set; }
    public string? Username { get; set; }
    public string? TokenType { get; set; }
    public long? Exp { get; set; }
    public long? Iat { get; set; }
    public long? Nbf { get; set; }
    public string? Sub { get; set; }
    public string? Aud { get; set; }
    public string? Iss { get; set; }
    public string? Jti { get; set; }
}

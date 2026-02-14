namespace OroIdentityServer.Server.Endpoints;

public record BackchannelLogoutRequest(string? Jti, string? RefreshToken);
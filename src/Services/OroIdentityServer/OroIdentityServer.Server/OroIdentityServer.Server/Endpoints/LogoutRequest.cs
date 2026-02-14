namespace OroIdentityServer.Server.Endpoints;

// Logout request DTOs
public record LogoutRequest(string? RefreshToken, string? PostLogoutRedirectUri);
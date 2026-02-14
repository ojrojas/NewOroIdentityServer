// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Models;

public class UserSession : BaseEntity<UserSession, UserSessionId>, IAuditableEntity, IAggregateRoot
{
    public UserSession(
        UserSessionId? id,
        UserId userId,
        string refreshToken,
        string ipAddress,
        string? country,
        DateTime loginTime,
        DateTime expiresAt)
    {
        Id = id ?? UserSessionId.New();
        UserId = userId;
        RefreshToken = refreshToken;
        IpAddress = ipAddress;
        Country = country;
        LoginTime = loginTime;
        ExpiresAt = expiresAt;
        IsActive = true;
    }

    private UserSession() { }

    public UserId UserId { get; private set; }
    public User? User { get; set; }
    public string RefreshToken { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public string? Country { get; private set; }
    public DateTime LoginTime { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsActive { get; private set; }

    public void Deactivate()
    {
        IsActive = false;
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;

    public void RotateRefreshToken(string newRefreshToken)
    {
        RefreshToken = newRefreshToken;
        LoginTime = DateTime.UtcNow;
    }
}
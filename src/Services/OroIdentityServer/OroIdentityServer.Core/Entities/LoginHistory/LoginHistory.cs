// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Models;

public class LoginHistory : BaseEntity<LoginHistory, LoginHistoryId>, IAuditableEntity, IAggregateRoot
{
    public LoginHistory(
        LoginHistoryId? id,
        UserId userId,
        string ipAddress,
        string? country,
        DateTime loginTime)
    {
        Id = id ?? LoginHistoryId.New();
        UserId = userId;
        IpAddress = ipAddress;
        Country = country;
        LoginTime = loginTime;
        IsActive = true;
    }

    private LoginHistory() { }

    public UserId UserId { get; private set; }
    public string IpAddress { get; private set; } = string.Empty;
    public string? Country { get; private set; }
    public DateTime LoginTime { get; private set; }
    public DateTime? LogoutTime { get; private set; }
    public bool IsActive { get; private set; }

    public User? User { get; set; }

    public TimeSpan? GetDuration()
    {
        if (!LogoutTime.HasValue) return null;
        return LogoutTime.Value - LoginTime;
    }

    public void Logout()
    {
        if (!IsActive) return;
        LogoutTime = DateTime.UtcNow;
        IsActive = false;
    }
}
// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Models;

public partial class SecurityUser : 
    BaseEntity<SecurityUser, SecurityUserId>, IAuditableEntity, IAggregateRoot
{
    public SecurityUser(string passwordHash, string securityStamp, string ConcurrencyStamp)
    {
        Id = new(Guid.CreateVersion7());
        PasswordHash = passwordHash;
        SecurityStamp = securityStamp;
        this.ConcurrencyStamp = ConcurrencyStamp;

        RaiseDomainEvent(new SecurityUserCreatedEvent(Id));
    }

    [MaxLength(256)]
    public string? PasswordHash { get; private set; }
    public string? SecurityStamp { get; private set; } = string.Empty;
    public string? ConcurrencyStamp { get; private set; }
    public DateTime? LockoutEnd { get; private set; } = DateTime.UtcNow;
    public bool LockoutEnabled { get; private set; } = false;
    public int AccessFailedCount { get; private set; } = 0;

    public bool IsLockedOut()
    {
        return LockoutEnabled && LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
    }

    public void IncrementAccessFailedCount()
    {
        AccessFailedCount++;
        RaiseDomainEvent(new AccessFailedIncrementedEvent(Id, AccessFailedCount));
    }

    // Override ResetAccessFailedCount to raise event
    public void ResetAccessFailedCount()
    {
        AccessFailedCount = 0;
        RaiseDomainEvent(new AccessFailedResetEvent(Id));
    }

    // Override LockUntil to raise event
    public void LockUntil(DateTime lockoutEnd)
    {
        LockoutEnd = lockoutEnd;
        LockoutEnabled = true;
        RaiseDomainEvent(new UserLockedEvent(Id, lockoutEnd));
    }

    // Add method to set LockoutEnabled with an event
    public void SetLockoutEnabled(bool enabled)
    {
        if (LockoutEnabled == enabled) return; // Avoid unnecessary updates

        LockoutEnabled = enabled;
        RaiseDomainEvent(new LockoutEnabledChangedEvent(Id, enabled));
    }

    // Add Create method
    public static SecurityUser Create(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("PasswordHash cannot be null or empty.");

        return new SecurityUser(
            passwordHash,
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
    }

    /// <summary>
    /// Cambia el hash de la contraseña del SecurityUser.
    /// </summary>
    /// <param name="hashedPassword">Contraseña ya hasheada.</param>
    public void ChangePassword(string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
            throw new ArgumentException("Hashed password cannot be null or empty.", nameof(hashedPassword));

        PasswordHash = hashedPassword;
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }
}

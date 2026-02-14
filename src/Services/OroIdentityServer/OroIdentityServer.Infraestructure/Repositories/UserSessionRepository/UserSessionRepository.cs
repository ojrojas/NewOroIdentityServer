// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Repositories;

public class UserSessionRepository(
    ILogger<UserSessionRepository> logger,
    IRepository<UserSession> repository) : IUserSessionRepository
{
    public async Task AddUserSessionAsync(UserSession userSession, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding user session for user: {UserId}", userSession.UserId.Value);
        await repository.AddAsync(userSession, cancellationToken);
    }

    public async Task<IEnumerable<UserSession>> GetActiveUserSessionsByUserIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving active user sessions for user: {UserId}", userId.Value);
        var now = DateTime.UtcNow;
        return await repository.FindAsync(us => us.UserId == userId && us.IsActive && us.ExpiresAt > now, cancellationToken);
    }

    public async Task<UserSession?> GetUserSessionByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving user session by refresh token");
        var now = DateTime.UtcNow;
        return await repository.FindSingleAsync(us => us.RefreshToken == refreshToken && us.IsActive && us.ExpiresAt > now, cancellationToken);
    }

    public async Task UpdateUserSessionAsync(UserSession userSession, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating user session for user: {UserId}", userSession.UserId.Value);
        await repository.UpdateAsync(userSession, cancellationToken);
    }
}
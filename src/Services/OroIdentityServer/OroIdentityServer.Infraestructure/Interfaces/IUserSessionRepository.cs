// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Interfaces;

/// <summary>
/// Represents a repository interface for managing user session entities.
/// </summary>
public interface IUserSessionRepository
{
    /// <summary>
    /// Asynchronously adds a new user session to the repository.
    /// </summary>
    /// <param name="userSession">The user session entity to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddUserSessionAsync(UserSession userSession, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves active user sessions for a user.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of active user sessions.</returns>
    Task<IEnumerable<UserSession>> GetActiveUserSessionsByUserIdAsync(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves a user session by refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user session if found; otherwise, null.</returns>
    Task<UserSession?> GetUserSessionByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously updates a user session.
    /// </summary>
    /// <param name="userSession">The user session to update.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateUserSessionAsync(UserSession userSession, CancellationToken cancellationToken);
}
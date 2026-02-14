// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Interfaces;

/// <summary>
/// Represents a repository interface for managing login history entities.
/// </summary>
public interface ILoginHistoryRepository
{
    /// <summary>
    /// Asynchronously adds a new login history to the repository.
    /// </summary>
    /// <param name="loginHistory">The login history entity to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddLoginHistoryAsync(LoginHistory loginHistory, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves login histories for a user.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of login histories.</returns>
    Task<IEnumerable<LoginHistory>> GetLoginHistoriesByUserIdAsync(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously updates a login history.
    /// </summary>
    /// <param name="loginHistory">The login history to update.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateLoginHistoryAsync(LoginHistory loginHistory, CancellationToken cancellationToken);

    /// <summary>
    /// Get a login history by id.
    /// </summary>
    Task<LoginHistory?> GetLoginHistoryByIdAsync(LoginHistoryId id, CancellationToken cancellationToken);
}
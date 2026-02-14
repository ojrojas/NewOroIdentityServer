// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Interfaces;

/// <summary>
/// Represents a repository interface for managing authorization code entities.
/// </summary>
public interface IAuthorizationRepository
{
    /// <summary>
    /// Asynchronously adds a new authorization to the repository.
    /// </summary>
    /// <param name="authorization">The authorization entity to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAuthorizationAsync(Authorization authorization, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves an authorization from the repository by code.
    /// </summary>
    /// <param name="code">The code of the authorization to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the authorization entity if found; otherwise, null.</returns>
    Task<Authorization?> GetAuthorizationByAsync(Guid Id, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously updates an existing authorization in the repository.
    /// </summary>
    /// <param name="authorization">The authorization entity with updated information.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAuthorizationAsync(Authorization authorization, CancellationToken cancellationToken);
}
// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Interfaces;

/// <summary>
/// Represents a repository interface for managing application entities.
/// </summary>
public interface IApplicationRepository
{
    /// <summary>
    /// Asynchronously adds a new application to the repository.
    /// </summary>
    /// <param name="application">The application entity to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAsync(Application application, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves an application by its ID.
    /// </summary>
    /// <param name="id">The ID of the application to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the application if found, otherwise null.</returns>
    Task<Application?> GetByIdAsync(Core.Models.ApplicationId id, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves all applications for a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the list of applications.</returns>
    Task<IEnumerable<Application>> GetByTenantIdAsync(TenantId tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously updates an existing application in the repository.
    /// </summary>
    /// <param name="application">The application entity to update.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAsync(Application application, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously deletes an application from the repository.
    /// </summary>
    /// <param name="application">The application entity to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteAsync(Application application, CancellationToken cancellationToken);
}
// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Repositories;

public class AuthorizationRepository(
    ILogger<AuthorizationRepository> logger,
    IRepository<Authorization> repository) : IAuthorizationRepository
{

    public async Task AddAuthorizationAsync(Authorization authorization, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding authorization code for application: {ApplicationId}", authorization.Id.Value);
        await repository.AddAsync(authorization, cancellationToken);
    }

    public async Task<Authorization?> GetAuthorizationByAsync(Guid id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving authorization by id: {Id}", id);
        var now = DateTime.UtcNow;
        return await repository.FindSingleAsync(ac => ac.Id.Value == id && ac.ExpiresAt > now, cancellationToken);
    }

    public async Task UpdateAuthorizationAsync(Authorization authorization, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating authorization: {Id}", authorization.Id.Value);
        await repository.UpdateAsync(authorization, cancellationToken);
    }
}
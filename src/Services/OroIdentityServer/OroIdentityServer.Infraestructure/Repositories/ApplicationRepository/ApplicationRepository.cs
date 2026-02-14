// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Repositories;

public class ApplicationRepository(
    ILogger<ApplicationRepository> logger,
    IRepository<Application> repository) : IApplicationRepository
{
    public async Task AddAsync(Application application, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding application: {Id}", application.Id);
        await repository.AddAsync(application, cancellationToken);
    }

    public async Task<Application?> GetByIdAsync(Core.Models.ApplicationId id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting application by id: {Id}", id);
        return await repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Application>> GetByTenantIdAsync(TenantId tenantId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting applications by tenant id: {TenantId}", tenantId);
        return await repository.CurrentContext
            .Where(a => a.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Application application, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating application: {Id}", application.Id);
        await repository.UpdateAsync(application, cancellationToken);
    }

    public async Task DeleteAsync(Application application, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting application: {Id}", application.Id);
        await repository.DeleteAsync(application, cancellationToken);
    }
}
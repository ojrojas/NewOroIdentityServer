// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Repositories;

public class LoginHistoryRepository(
    ILogger<LoginHistoryRepository> logger,
    IRepository<LoginHistory> repository) : ILoginHistoryRepository
{
    public async Task AddLoginHistoryAsync(LoginHistory loginHistory, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding login history for user: {UserId}", loginHistory.UserId.Value);
        await repository.AddAsync(loginHistory, cancellationToken);
    }

    public async Task<IEnumerable<LoginHistory>> GetLoginHistoriesByUserIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving login histories for user: {UserId}", userId.Value);
        return await repository.FindAsync(lh => lh.UserId == userId, cancellationToken);
    }

    public async Task UpdateLoginHistoryAsync(LoginHistory loginHistory, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating login history for user: {UserId}", loginHistory.UserId.Value);
        await repository.UpdateAsync(loginHistory, cancellationToken);
    }

    public async Task<LoginHistory?> GetLoginHistoryByIdAsync(LoginHistoryId id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving login history by id: {Id}", id.Value);
        return await repository.GetByIdAsync(id, cancellationToken);
    }
}
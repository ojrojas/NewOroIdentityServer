using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using OroIdentityServer.Core.Models;
using OroIdentityServer.Infraestructure.Repositories;

namespace OroIdentityServer.Infraestructure.Tests.Logout;

public class LogoutIntegrationTests
{
    [Fact]
    public async Task UpdateUserSessionCommand_DeactivatesSession()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var repo = new Repository<UserSession>(NullLogger<Repository<UserSession>>.Instance, context);
        var userSessionRepo = new OroIdentityServer.Infraestructure.Repositories.UserSessionRepository(NullLogger<OroIdentityServer.Infraestructure.Repositories.UserSessionRepository>.Instance, repo);

        // create a session and persist
        var userId = UserId.New();
        var refresh = "logout-refresh-123";
        var session = new UserSession(null, userId, refresh, "127.0.0.1", null, DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
        await userSessionRepo.AddUserSessionAsync(session, CancellationToken.None);

        // ensure active
        var stored = await userSessionRepo.GetUserSessionByRefreshTokenAsync(refresh, CancellationToken.None);
        Assert.NotNull(stored);
        Assert.True(stored!.IsActive);

        // Deactivate session directly via repository (infra tests must not reference Application project types)
        var toUpdate = await userSessionRepo.GetUserSessionByRefreshTokenAsync(refresh, CancellationToken.None);
        Assert.NotNull(toUpdate);
        toUpdate!.Deactivate();
        await userSessionRepo.UpdateUserSessionAsync(toUpdate, CancellationToken.None);

        var after = await userSessionRepo.GetUserSessionByRefreshTokenAsync(refresh, CancellationToken.None);
        Assert.Null(after); // repository filters inactive/expired
    }
}

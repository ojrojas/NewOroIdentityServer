using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using OroIdentityServer.Core.Models;
using OroIdentityServer.Infraestructure.Repositories;

namespace OroIdentityServer.Infraestructure.Tests.Repositories;

public class UserSessionRepositoryTests
{
    [Fact]
    public async Task Add_GetActive_GetByRefreshToken_Update_Works()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var repo = new Repository<UserSession>(NullLogger<Repository<UserSession>>.Instance, context);
        var usRepo = new UserSessionRepository(NullLogger<UserSessionRepository>.Instance, repo);

        var session = new UserSession(null, UserId.New(), "ref-1", "127.0.0.1", "Country", DateTime.UtcNow, DateTime.UtcNow.AddMinutes(30));
        await usRepo.AddUserSessionAsync(session, CancellationToken.None);

        var active = (await usRepo.GetActiveUserSessionsByUserIdAsync(session.UserId, CancellationToken.None)).ToList();
        Assert.Single(active);

        var byToken = await usRepo.GetUserSessionByRefreshTokenAsync("ref-1", CancellationToken.None);
        Assert.NotNull(byToken);

        session.Deactivate();
        await usRepo.UpdateUserSessionAsync(session, CancellationToken.None);

        var after = (await usRepo.GetActiveUserSessionsByUserIdAsync(session.UserId, CancellationToken.None)).ToList();
        Assert.Empty(after);
    }

    [Fact]
    public async Task ExpiredUserSession_IsNotReturned()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var repo = new Repository<UserSession>(NullLogger<Repository<UserSession>>.Instance, context);
        var usRepo = new UserSessionRepository(NullLogger<UserSessionRepository>.Instance, repo);

        var expired = new UserSession(null, UserId.New(), "ref-expired", "127.0.0.1", "Country", DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(-1));
        await usRepo.AddUserSessionAsync(expired, CancellationToken.None);

        var active = (await usRepo.GetActiveUserSessionsByUserIdAsync(expired.UserId, CancellationToken.None)).ToList();
        Assert.Empty(active);

        var byToken = await usRepo.GetUserSessionByRefreshTokenAsync("ref-expired", CancellationToken.None);
        Assert.Null(byToken);
    }
}
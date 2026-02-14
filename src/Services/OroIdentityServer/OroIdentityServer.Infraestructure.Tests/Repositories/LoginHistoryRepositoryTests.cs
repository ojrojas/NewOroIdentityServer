using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using OroIdentityServer.Core.Models;
using OroIdentityServer.Infraestructure.Repositories;

namespace OroIdentityServer.Infraestructure.Tests.Repositories;

public class LoginHistoryRepositoryTests
{
    [Fact]
    public async Task Add_Get_Update_LoginHistory_Works()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var repo = new Repository<LoginHistory>(NullLogger<Repository<LoginHistory>>.Instance, context);
        var loginRepo = new LoginHistoryRepository(NullLogger<LoginHistoryRepository>.Instance, repo);

        var userId = UserId.New();
        var login = new LoginHistory(null, userId, "127.0.0.1", "Country", DateTime.UtcNow);

        await loginRepo.AddLoginHistoryAsync(login, CancellationToken.None);

        var histories = (await loginRepo.GetLoginHistoriesByUserIdAsync(userId, CancellationToken.None)).ToList();
        Assert.Single(histories);
        Assert.Equal("127.0.0.1", histories[0].IpAddress);

        // Logout and update
        login.Logout();
        await loginRepo.UpdateLoginHistoryAsync(login, CancellationToken.None);

        var updated = (await loginRepo.GetLoginHistoriesByUserIdAsync(userId, CancellationToken.None)).First();
        Assert.False(updated.IsActive);
        Assert.NotNull(updated.LogoutTime);
    }
}
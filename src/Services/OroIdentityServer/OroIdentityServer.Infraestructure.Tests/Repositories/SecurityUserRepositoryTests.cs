using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using OroIdentityServer.Core.Models;
using OroIdentityServer.Infraestructure.Repositories;

namespace OroIdentityServer.Infraestructure.Tests.Repositories;

public class SecurityUserRepositoryTests
{
    [Fact]
    public async Task GetSecurityUserAsync_ReturnsSecurityUser()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var repo = new Repository<SecurityUser>(NullLogger<Repository<SecurityUser>>.Instance, context);
        var securityRepo = new SecurityUserRepository(NullLogger<SecurityUserRepository>.Instance, repo);

        var su = SecurityUser.Create("hash");
        await repo.AddAsync(su, CancellationToken.None);

        var fetched = await securityRepo.GetSecurityUserAsync(su.Id.Value, CancellationToken.None);
        Assert.Equal(su.Id.Value, fetched.Id.Value);
        Assert.Equal(su.PasswordHash, fetched.PasswordHash);
    }
}
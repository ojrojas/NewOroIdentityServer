using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using OroIdentityServer.Core.Models;
using OroIdentityServer.Infraestructure.Repositories;

namespace OroIdentityServer.Infraestructure.Tests.Repositories;

public class AuthorizationRepositoryTests
{
    [Fact]
    public async Task Add_Get_Update_Authorization_Works()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var appRepoGeneric = new Repository<Application>(NullLogger<Repository<Application>>.Instance, context);
        var authRepoGeneric = new Repository<Authorization>(NullLogger<Repository<Authorization>>.Instance, context);

        var app = new Application(null, ClientSecret.New(), new List<string>{"u"}, new List<string>{"g"}, new List<string>{"s"}, TenantId.New());
        await appRepoGeneric.AddAsync(app, CancellationToken.None);

        var authRepo = new AuthorizationRepository(NullLogger<AuthorizationRepository>.Instance, authRepoGeneric);

        var auth = new Authorization(app, DateTime.UtcNow.AddHours(1), "scopes", "active", "type");
        await authRepo.AddAuthorizationAsync(auth, CancellationToken.None);

        var fetched = await authRepo.GetAuthorizationByAsync(auth.Id.Value, CancellationToken.None);
        Assert.NotNull(fetched);
        Assert.Equal("active", fetched!.Status);

        fetched.UpdateStatus("inactive");
        await authRepo.UpdateAuthorizationAsync(fetched, CancellationToken.None);

        var updated = await authRepo.GetAuthorizationByAsync(fetched.Id.Value, CancellationToken.None);
        Assert.Equal("inactive", updated?.Status);
    }

    [Fact]
    public async Task GetAuthorizationByAsync_Expired_ReturnsNull()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var appRepoGeneric = new Repository<Application>(NullLogger<Repository<Application>>.Instance, context);
        var authRepoGeneric = new Repository<Authorization>(NullLogger<Repository<Authorization>>.Instance, context);

        var app = new Application(null, ClientSecret.New(), new List<string>{"u"}, new List<string>{"g"}, new List<string>{"s"}, TenantId.New());
        await appRepoGeneric.AddAsync(app, CancellationToken.None);

        var authRepo = new AuthorizationRepository(NullLogger<AuthorizationRepository>.Instance, authRepoGeneric);

        var expiredAuth = new Authorization(app, DateTime.UtcNow.AddHours(-1), "scopes", "active", "type");
        await authRepo.AddAuthorizationAsync(expiredAuth, CancellationToken.None);

        var fetched = await authRepo.GetAuthorizationByAsync(expiredAuth.Id.Value, CancellationToken.None);
        Assert.Null(fetched);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using OroIdentityServer.Core.Models;
using OroIdentityServer.Infraestructure.Repositories;

namespace OroIdentityServer.Infraestructure.Tests.Repositories;

public class ApplicationRepositoryTests
{
    [Fact]
    public async Task Add_GetByTenantId_Update_Delete_Application_Works()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var repo = new Repository<Application>(NullLogger<Repository<Application>>.Instance, context);
        var appRepo = new ApplicationRepository(NullLogger<ApplicationRepository>.Instance, repo);

        var app = new Application(null, ClientSecret.New(), new List<string>{"u"}, new List<string>{"g"}, new List<string>{"s"}, TenantId.New());
        await appRepo.AddAsync(app, CancellationToken.None);

        var list = (await appRepo.GetByTenantIdAsync(app.TenantId, CancellationToken.None)).ToList();
        Assert.Single(list);

        app.Deactivate();
        await appRepo.UpdateAsync(app, CancellationToken.None);

        var fetched = await appRepo.GetByIdAsync(app.Id, CancellationToken.None);
        Assert.False(fetched!.IsActive);

        await appRepo.DeleteAsync(app, CancellationToken.None);
        var after = await repo.GetByIdAsync(app.Id, CancellationToken.None);
        Assert.Null(after);
    }
}
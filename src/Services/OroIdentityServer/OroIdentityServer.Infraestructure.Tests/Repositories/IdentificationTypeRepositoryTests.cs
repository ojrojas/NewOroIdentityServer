using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using OroIdentityServer.Core.Models;
using OroIdentityServer.Infraestructure.Repositories;

namespace OroIdentityServer.Infraestructure.Tests.Repositories;

public class IdentificationTypeRepositoryTests
{
    [Fact]
    public async Task Add_Get_Update_Delete_IdentificationType_Works()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var repo = new Repository<IdentificationType>(NullLogger<Repository<IdentificationType>>.Instance, context);
        var idRepo = new IdentificationTypeRepository(NullLogger<IdentificationTypeRepository>.Instance, repo);

        var it = new IdentificationType("Passport");
        await idRepo.AddIdentificationTypeAsync(it, CancellationToken.None);

        var fetched = await idRepo.GetIdentificationTypeByNameAsync(IdentificationTypeName.Create("Passport"), CancellationToken.None);
        Assert.NotNull(fetched);
        Assert.Equal("Passport", fetched!.Name.Value);

        fetched.UpdateName(IdentificationTypeName.Create("IDCard"));
        await idRepo.UpdateIdentificationTypeAsync(fetched, CancellationToken.None);

        var updated = await idRepo.GetIdentificationTypeByIdAsync(fetched.Id, CancellationToken.None);
        Assert.Equal("IDCard", updated!.Name.Value);

        await idRepo.DeleteIdentificationTypeAsync(fetched.Id, CancellationToken.None);
        var afterDelete = await repo.GetByIdAsync(fetched.Id, CancellationToken.None);
        Assert.Null(afterDelete);
    }
}
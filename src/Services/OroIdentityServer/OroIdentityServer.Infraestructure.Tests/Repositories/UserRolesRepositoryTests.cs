using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using OroIdentityServer.Core.Models;
using OroIdentityServer.Infraestructure.Repositories;

namespace OroIdentityServer.Infraestructure.Tests.Repositories;

public class UserRolesRepositoryTests
{
    [Fact]
    public async Task Add_Get_Delete_UserRoles_Works()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var repo = new Repository<UserRole>(NullLogger<Repository<UserRole>>.Instance, context);
        var userRolesRepo = new UserRolesRepository(NullLogger<UserRolesRepository>.Instance, repo);

        var userId = UserId.New();
        var roleId = RoleId.New();

        var ur = new UserRole(userId, roleId);
        await userRolesRepo.AddUserRoleAsync(ur, CancellationToken.None);

        var roles = (await userRolesRepo.GetRolesByUserIdAsync(userId, CancellationToken.None)).ToList();
        Assert.Single(roles);

        await userRolesRepo.DeleteUserRoleAsync(ur, CancellationToken.None);
        var rolesAfter = (await userRolesRepo.GetRolesByUserIdAsync(userId, CancellationToken.None)).ToList();
        Assert.Empty(rolesAfter);
    }
}
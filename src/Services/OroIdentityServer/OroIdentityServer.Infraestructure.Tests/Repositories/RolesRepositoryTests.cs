using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using OroIdentityServer.Core.Models;
using OroIdentityServer.Infraestructure.Repositories;

namespace OroIdentityServer.Infraestructure.Tests.Repositories;

public class RolesRepositoryTests
{
    [Fact]
    public async Task Add_GetRoleByName_AddAndRemoveClaims_Works()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var repo = new Repository<Role>(NullLogger<Repository<Role>>.Instance, context);
        var rolesRepo = new RolesRepository(NullLogger<RolesRepository>.Instance, repo);

        var role = new Role("Admin");
        await repo.AddAsync(role, CancellationToken.None);

        var fetched = await rolesRepo.GetRoleByNameAsync("Admin", CancellationToken.None);
        Assert.NotNull(fetched);
        Assert.Equal(role.Name?.Value, fetched?.Name?.Value);

        // Add claim
        await rolesRepo.AddRoleClaimAsync(role.Id, new RoleClaimType("type"), new RoleClaimValue("value"), CancellationToken.None);
        var claims = await rolesRepo.GetRoleClaimsByRoleIdAsync(role.Id, CancellationToken.None);
        Assert.Single(claims);

        var claimId = claims.First().Id;
        await rolesRepo.DeleteRoleClaimAsync(claimId, CancellationToken.None);
        var claimsAfter = await rolesRepo.GetRoleClaimsByRoleIdAsync(role.Id, CancellationToken.None);
        Assert.Empty(claimsAfter);
    }

    [Fact]
    public async Task DeleteRoleClaim_NonExistent_DoesNotThrowOrChange()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var repo = new Repository<Role>(NullLogger<Repository<Role>>.Instance, context);
        var rolesRepo = new RolesRepository(NullLogger<RolesRepository>.Instance, repo);

        var role = new Role("Admin");
        await repo.AddAsync(role, CancellationToken.None);

        await rolesRepo.AddRoleClaimAsync(role.Id, new RoleClaimType("type"), new RoleClaimValue("value"), CancellationToken.None);
        var before = (await rolesRepo.GetRoleClaimsByRoleIdAsync(role.Id, CancellationToken.None)).ToList();
        Assert.Single(before);

        await rolesRepo.DeleteRoleClaimAsync(Guid.NewGuid(), CancellationToken.None);

        var after = (await rolesRepo.GetRoleClaimsByRoleIdAsync(role.Id, CancellationToken.None)).ToList();
        Assert.Single(after);
    }

    [Fact]
    public async Task UpdateRoleClaim_NonExistent_DoesNotThrowOrChange()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var repo = new Repository<Role>(NullLogger<Repository<Role>>.Instance, context);
        var rolesRepo = new RolesRepository(NullLogger<RolesRepository>.Instance, repo);

        var role = new Role("Admin");
        await repo.AddAsync(role, CancellationToken.None);

        await rolesRepo.AddRoleClaimAsync(role.Id, new RoleClaimType("type"), new RoleClaimValue("value"), CancellationToken.None);
        var before = (await rolesRepo.GetRoleClaimsByRoleIdAsync(role.Id, CancellationToken.None)).ToList();
        Assert.Single(before);

        await rolesRepo.UpdateRoleClaimAsync(Guid.NewGuid(), new RoleClaimType("t2"), new RoleClaimValue("v2"), CancellationToken.None);

        var after = (await rolesRepo.GetRoleClaimsByRoleIdAsync(role.Id, CancellationToken.None)).ToList();
        Assert.Single(after);
    }
}
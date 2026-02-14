using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using OroIdentityServer.Core.Models;
using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Infraestructure.Repositories;

namespace OroIdentityServer.Infraestructure.Tests.Repositories;

public class UserRepositoryTests
{
    [Fact]
    public async Task Add_Get_Update_Delete_User_Works()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var repo = new Repository<User>(NullLogger<Repository<User>>.Instance, context);
        var securityRepo = new SecurityUserRepository(NullLogger<SecurityUserRepository>.Instance, new Repository<SecurityUser>(NullLogger<Repository<SecurityUser>>.Instance, context));

        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        passwordHasherMock.Setup(p => p.HashPassword(It.IsAny<string>())).ReturnsAsync("newhash");

        var userRepository = new UserRepository(NullLogger<UserRepository>.Instance, repo, securityRepo, passwordHasherMock.Object);

        var user = User.Create("u1", "u1@example.com", "Name", "M", "L", "ident", IdentificationTypeId.New(), TenantId.New());
        await userRepository.AddUserAsync(user, CancellationToken.None);

        var fetched = await userRepository.GetUserByEmailAsync(user.Email!, CancellationToken.None);
        Assert.Equal(user.UserName, fetched.UserName);

        // Update
        fetched.UpdateDetails("NewName", fetched.MiddleName!, fetched.LastName!, fetched.UserName!, fetched.Email!, fetched.Identification!, fetched.IdentificationTypeId!, fetched.TenantId!);
        await userRepository.UpdateUserAsync(fetched, CancellationToken.None);
        var updated = await userRepository.GetUserByIdAsync(fetched.Id, CancellationToken.None);
        Assert.Equal("NewName", updated!.Name);

        // Delete
        await userRepository.DeleteUserAsync(fetched.Id, CancellationToken.None);
        var afterDelete = await repo.GetByIdAsync(fetched.Id, CancellationToken.None);
        Assert.Null(afterDelete);
    }

    [Fact]
    public async Task ValidateUserCanLoginAsync_LockedOut_ReturnsFalse()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var repo = new Repository<User>(NullLogger<Repository<User>>.Instance, context);
        var secRepoGeneric = new Repository<SecurityUser>(NullLogger<Repository<SecurityUser>>.Instance, context);
        var securityRepo = new SecurityUserRepository(NullLogger<SecurityUserRepository>.Instance, secRepoGeneric);

        var passwordHasher = new Mock<IPasswordHasher>();

        var userRepository = new UserRepository(NullLogger<UserRepository>.Instance, repo, securityRepo, passwordHasher.Object);

        var su = SecurityUser.Create("hash");
        su.LockUntil(DateTime.UtcNow.AddHours(1));
        await secRepoGeneric.AddAsync(su, CancellationToken.None);

        var user = User.Create("u2", "u2@example.com", "Name", "M", "L", "ident", IdentificationTypeId.New(), TenantId.New());
        user.AssignSecurityUser(su);
        await repo.AddAsync(user, CancellationToken.None);

        var canLogin = await userRepository.ValidateUserCanLoginAsync(user.Email!, CancellationToken.None);
        Assert.False(canLogin);
    }

    [Fact]
    public async Task ChangePasswordAsync_UpdatesSecurityUserPassword()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var repo = new Repository<User>(NullLogger<Repository<User>>.Instance, context);
        var secRepoGeneric = new Repository<SecurityUser>(NullLogger<Repository<SecurityUser>>.Instance, context);
        var securityRepo = new SecurityUserRepository(NullLogger<SecurityUserRepository>.Instance, secRepoGeneric);

        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock.Setup(p => p.VerifyPassword("current", "oldhash")).ReturnsAsync(true);
        passwordHasherMock.Setup(p => p.HashPassword("newpass")).ReturnsAsync("newhash");

        var userRepository = new UserRepository(NullLogger<UserRepository>.Instance, repo, securityRepo, passwordHasherMock.Object);

        var su = SecurityUser.Create("oldhash");
        await secRepoGeneric.AddAsync(su, CancellationToken.None);

        var user = User.Create("u3", "u3@example.com", "Name", "M", "L", "ident", IdentificationTypeId.New(), TenantId.New());
        user.AssignSecurityUser(su);
        await repo.AddAsync(user, CancellationToken.None);

        var result = await userRepository.ChangePasswordAsync(user.Email!, "current", "newpass", "newpass", CancellationToken.None);
        Assert.True(result);

        var updatedSu = await secRepoGeneric.GetByIdAsync(su.Id, CancellationToken.None);
        Assert.Equal("newhash", updatedSu!.PasswordHash);
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongCurrentPassword_ReturnsFalse()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var repo = new Repository<User>(NullLogger<Repository<User>>.Instance, context);
        var secRepoGeneric = new Repository<SecurityUser>(NullLogger<Repository<SecurityUser>>.Instance, context);
        var securityRepo = new SecurityUserRepository(NullLogger<SecurityUserRepository>.Instance, secRepoGeneric);

        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock.Setup(p => p.VerifyPassword("wrong", "oldhash")).ReturnsAsync(false);

        var userRepository = new UserRepository(NullLogger<UserRepository>.Instance, repo, securityRepo, passwordHasherMock.Object);

        var su = SecurityUser.Create("oldhash");
        await secRepoGeneric.AddAsync(su, CancellationToken.None);

        var user = User.Create("u4", "u4@example.com", "Name", "M", "L", "ident", IdentificationTypeId.New(), TenantId.New());
        user.AssignSecurityUser(su);
        await repo.AddAsync(user, CancellationToken.None);

        var result = await userRepository.ChangePasswordAsync(user.Email!, "wrong", "newpass", "newpass", CancellationToken.None);
        Assert.False(result);
    }

    [Fact]
    public async Task ChangePasswordAsync_PasswordsDoNotMatch_ReturnsFalse()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var repo = new Repository<User>(NullLogger<Repository<User>>.Instance, context);
        var secRepoGeneric = new Repository<SecurityUser>(NullLogger<Repository<SecurityUser>>.Instance, context);
        var securityRepo = new SecurityUserRepository(NullLogger<SecurityUserRepository>.Instance, secRepoGeneric);

        var passwordHasherMock = new Mock<IPasswordHasher>();

        var userRepository = new UserRepository(NullLogger<UserRepository>.Instance, repo, securityRepo, passwordHasherMock.Object);

        var su = SecurityUser.Create("oldhash");
        await secRepoGeneric.AddAsync(su, CancellationToken.None);

        var user = User.Create("u5", "u5@example.com", "Name", "M", "L", "ident", IdentificationTypeId.New(), TenantId.New());
        user.AssignSecurityUser(su);
        await repo.AddAsync(user, CancellationToken.None);

        var result = await userRepository.ChangePasswordAsync(user.Email!, "current", "newpass", "mismatch", CancellationToken.None);
        Assert.False(result);
    }

    [Fact]
    public async Task ChangePasswordAsync_UserWithoutSecurityUser_ReturnsFalse()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var repo = new Repository<User>(NullLogger<Repository<User>>.Instance, context);
        var secRepoGeneric = new Repository<SecurityUser>(NullLogger<Repository<SecurityUser>>.Instance, context);
        var securityRepo = new SecurityUserRepository(NullLogger<SecurityUserRepository>.Instance, secRepoGeneric);

        var passwordHasherMock = new Mock<IPasswordHasher>();

        var userRepository = new UserRepository(NullLogger<UserRepository>.Instance, repo, securityRepo, passwordHasherMock.Object);

        var user = User.Create("u6", "u6@example.com", "Name", "M", "L", "ident", IdentificationTypeId.New(), TenantId.New());
        await repo.AddAsync(user, CancellationToken.None);

        var result = await userRepository.ChangePasswordAsync(user.Email!, "current", "newpass", "newpass", CancellationToken.None);
        Assert.False(result);
    }
}
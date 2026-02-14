using Moq;
using Xunit;
using OroIdentityServer.Application.Queries;
using OroIdentityServer.Infraestructure.Interfaces;
using OroIdentityServer.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using System.Threading;
using OroIdentityServer.Core.Interfaces;
using System;

namespace OroIdentityServer.Application.Tests.Queries;

public class ValidateUserPasswordHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsTrue_WhenPasswordValid()
    {
        var user = User.Create("u1", "u1@example.com", "Name", "M", "L", "ident", IdentificationTypeId.New(), TenantId.New());
        var su = SecurityUser.Create("hash");
        user.AssignSecurityUser(su);

        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var secRepo = new Mock<ISecurityUserRepository>();
        secRepo.Setup(r => r.GetSecurityUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(su);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var handler = new ValidateUserPasswordHandler(NullLogger<ValidateUserPasswordHandler>.Instance, userRepo.Object, secRepo.Object, hasher.Object);

        var resp = await handler.HandleAsync(new ValidateUserPasswordQuery("u1@example.com", "pwd"), CancellationToken.None);
        Assert.True(resp.Data);
    }

    [Fact]
    public async Task HandleAsync_ReturnsFalse_WhenPasswordInvalidOrSecurityUserMissing()
    {
        var user = User.Create("u2", "u2@example.com", "Name", "M", "L", "ident", IdentificationTypeId.New(), TenantId.New());
        user.AssignSecurityUser(SecurityUser.Create("hash"));

        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var secRepo = new Mock<ISecurityUserRepository>();
        secRepo.Setup(r => r.GetSecurityUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((SecurityUser?)null);

        var hasher = new Mock<IPasswordHasher>();

        var handler = new ValidateUserPasswordHandler(NullLogger<ValidateUserPasswordHandler>.Instance, userRepo.Object, secRepo.Object, hasher.Object);

        var resp = await handler.HandleAsync(new ValidateUserPasswordQuery("u2@example.com", "pwd"), CancellationToken.None);
        Assert.False(resp.Data);
    }
}
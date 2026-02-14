using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using System.Threading.Tasks;
using OroIdentityServer.Application.Commands;
using OroIdentityServer.Infraestructure.Interfaces;
using OroIdentityServer.Core.Models;
using System.Threading;
using System;
using OroIdentityServer.Core.Interfaces;

namespace OroIdentityServer.Application.Tests.Handlers;

public class LoginUserCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenCredentialsValid_UpdatesUserAndReturns()
    {
        var user = User.Create("u1", "u1@example.com", "Name", "M", "L", "ident", IdentificationTypeId.New(), TenantId.New());
        var su = SecurityUser.Create("hash");
        user.AssignSecurityUser(su);

        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        repo.Setup(r => r.UpdateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var handler = new LoginUserCommandHandler(NullLogger<LoginUserCommandHandler>.Instance, repo.Object, hasher.Object);

        await handler.HandleAsync(new LoginUserCommand(user.Email!, "pwd"), CancellationToken.None);

        repo.Verify(r => r.UpdateUserAsync(It.Is<User>(u => u.Id == user.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThrowsUnauthorizedAccessException()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var hasher = new Mock<IPasswordHasher>();

        var handler = new LoginUserCommandHandler(NullLogger<LoginUserCommandHandler>.Instance, repo.Object, hasher.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(new LoginUserCommand("noone@example.com", "pwd"), CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenPasswordIncorrect_IncrementsFailedCountAndThrows()
    {
        var user = User.Create("u2", "u2@example.com", "Name", "M", "L", "ident", IdentificationTypeId.New(), TenantId.New());
        var su = SecurityUser.Create("hash");
        user.AssignSecurityUser(su);

        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        repo.Setup(r => r.UpdateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        var handler = new LoginUserCommandHandler(NullLogger<LoginUserCommandHandler>.Instance, repo.Object, hasher.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(new LoginUserCommand(user.Email!, "bad"), CancellationToken.None));

        repo.Verify(r => r.UpdateUserAsync(It.Is<User>(u => u.SecurityUser!.AccessFailedCount > 0), It.IsAny<CancellationToken>()), Times.Once);
    }
}
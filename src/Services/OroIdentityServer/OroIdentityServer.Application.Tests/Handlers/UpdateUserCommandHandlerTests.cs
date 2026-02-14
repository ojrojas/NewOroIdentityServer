using Moq;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using OroIdentityServer.Application.Commands;
using OroIdentityServer.Infraestructure.Interfaces;
using OroIdentityServer.Core.Models;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace OroIdentityServer.Application.Tests.Handlers;

public class UpdateUserCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserExists_UpdatesAndReturnsResponse()
    {
        var user = User.Create("u1", "u1@example.com", "Name", "M", "L", "ident", IdentificationTypeId.New(), TenantId.New());
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetUserByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        repo.Setup(r => r.UpdateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new UpdateUserCommandHandler(NullLogger<UpdateUserCommandHandler>.Instance, repo.Object);

        var cmd = new UpdateUserCommand(user.Id, "New", "M", "L", "u1", "u1@example.com", "ident", "12",IdentificationTypeId.New(), TenantId.New());

        var resp = await handler.HandleAsync(cmd, CancellationToken.None);

        Assert.Equal(200, resp.StatusCode);
        Assert.Equal(user.Id, resp.Data.Id);
        repo.Verify(r => r.UpdateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUserMissing_ThrowsInvalidOperationException()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetUserByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var handler = new UpdateUserCommandHandler(NullLogger<UpdateUserCommandHandler>.Instance, repo.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(
            new UpdateUserCommand(UserId.New(), "N", "M", "L", "u", "e@e.com", "ident","12", IdentificationTypeId.New(), TenantId.New()), CancellationToken.None));
    }
}
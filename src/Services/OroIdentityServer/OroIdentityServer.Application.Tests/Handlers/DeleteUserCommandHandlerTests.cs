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

public class DeleteUserCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserExists_DeletesUser()
    {
        var user = User.Create("u1", "u1@example.com", "Name", "M", "L", "ident", IdentificationTypeId.New(), TenantId.New());
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetUserByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        repo.Setup(r => r.DeleteUserAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new DeleteUserCommandHander(NullLogger<DeleteUserCommandHander>.Instance, repo.Object);

        await handler.HandleAsync(new DeleteUserCommand(user.Id), CancellationToken.None);

        repo.Verify(r => r.DeleteUserAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUserMissing_ThrowsInvalidOperationException()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetUserByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var handler = new DeleteUserCommandHander(NullLogger<DeleteUserCommandHander>.Instance, repo.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(new DeleteUserCommand(UserId.New()), CancellationToken.None));
    }
}
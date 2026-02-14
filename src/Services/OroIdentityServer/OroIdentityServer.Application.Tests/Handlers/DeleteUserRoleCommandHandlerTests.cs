using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using System.Threading.Tasks;
using OroIdentityServer.Application.Commands;
using OroIdentityServer.Infraestructure.Interfaces;
using OroIdentityServer.Core.Models;
using System.Threading;
using System;

namespace OroIdentityServer.Application.Tests.Handlers;

public class DeleteUserRoleCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_CallsDeleteUserRole()
    {
        var repo = new Mock<IUserRolesRepository>();
        repo.Setup(r => r.DeleteUserRoleAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new DeleteUserRoleCommandHandler(NullLogger<DeleteUserRoleCommandHandler>.Instance, repo.Object);

        var userId = UserId.New();
        var roleId = RoleId.New();

        await handler.HandleAsync(new DeleteUserRoleCommand(userId, roleId), CancellationToken.None);

        repo.Verify(r => r.DeleteUserRoleAsync(It.Is<UserRole>(ur => ur.UserId == userId && ur.RoleId == roleId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_RepositoryThrows_Propagates()
    {
        var repo = new Mock<IUserRolesRepository>();
        repo.Setup(r => r.DeleteUserRoleAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException());

        var handler = new DeleteUserRoleCommandHandler(NullLogger<DeleteUserRoleCommandHandler>.Instance, repo.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(new DeleteUserRoleCommand(UserId.New(), RoleId.New()), CancellationToken.None));
    }
}

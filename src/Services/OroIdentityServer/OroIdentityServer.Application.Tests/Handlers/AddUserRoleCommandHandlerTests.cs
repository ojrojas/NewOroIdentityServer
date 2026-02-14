using Moq;
using Xunit;
using System.Threading.Tasks;
using OroIdentityServer.Application.Commands;
using OroIdentityServer.Infraestructure.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using OroIdentityServer.Core.Models;
using System.Threading;
using System;

namespace OroIdentityServer.Application.Tests.Handlers;

public class AddUserRoleCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenCalled_AddsUserRole()
    {
        var repo = new Mock<IUserRolesRepository>();
        repo.Setup(r => r.AddUserRoleAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new AddUserRoleCommandHandler(NullLogger<AddUserRoleCommandHandler>.Instance, repo.Object);

        var cmd = new AddUserRoleCommand(UserId.New(), RoleId.New());

        await handler.HandleAsync(cmd, CancellationToken.None);

        repo.Verify(r => r.AddUserRoleAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_RepositoryThrows_Propagates()
    {
        var repo = new Mock<IUserRolesRepository>();
        repo.Setup(r => r.AddUserRoleAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException());

        var handler = new AddUserRoleCommandHandler(NullLogger<AddUserRoleCommandHandler>.Instance, repo.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(new AddUserRoleCommand(UserId.New(), RoleId.New()), CancellationToken.None));
    }
}
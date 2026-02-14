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

public class DeleteRoleCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenRoleExists_DeletesRole()
    {
        var role = new Role("Admin");
        var repo = new Mock<IRolesRepository>();
        repo.Setup(r => r.GetRoleByIdAsync(role.Id, It.IsAny<CancellationToken>())).ReturnsAsync(role);
        repo.Setup(r => r.DeleteRoleAsync(role.Id, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new DeleteRoleCommandHandler(repo.Object, NullLogger<DeleteRoleCommandHandler>.Instance);

        await handler.HandleAsync(new DeleteRoleCommand(role.Id), CancellationToken.None);

        repo.Verify(r => r.DeleteRoleAsync(role.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRoleNotFound_Throws()
    {
        var repo = new Mock<IRolesRepository>();
        repo.Setup(r => r.GetRoleByIdAsync(It.IsAny<RoleId>(), It.IsAny<CancellationToken>())).ReturnsAsync((Role?)null);

        var handler = new DeleteRoleCommandHandler(repo.Object, NullLogger<DeleteRoleCommandHandler>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(new DeleteRoleCommand(RoleId.New()), CancellationToken.None));
    }
}

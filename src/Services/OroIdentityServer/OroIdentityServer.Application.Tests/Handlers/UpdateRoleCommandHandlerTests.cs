using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using System.Threading.Tasks;
using OroIdentityServer.Application.Commands;
using OroIdentityServer.Infraestructure.Interfaces;
using OroIdentityServer.Core.Models;
using System.Threading;
using System.Collections.Generic;

namespace OroIdentityServer.Application.Tests.Handlers;

public class UpdateRoleCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenRoleExists_UpdatesRole()
    {
        var role = new Role("User");
        var repo = new Mock<IRolesRepository>();
        repo.Setup(r => r.GetRoleByIdAsync(role.Id, It.IsAny<CancellationToken>())).ReturnsAsync(role);
        repo.Setup(r => r.UpdateRoleAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new UpdateRoleCommandHandler(NullLogger<UpdateRoleCommandHandler>.Instance, repo.Object);

        await handler.HandleAsync(new UpdateRoleCommand(role.Id, new RoleName("NewName")), CancellationToken.None);

        repo.Verify(r => r.UpdateRoleAsync(It.Is<Role>(x => x.Name.Value == "NewName"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRoleNotFound_ThrowsKeyNotFoundException()
    {
        var repo = new Mock<IRolesRepository>();
        repo.Setup(r => r.GetRoleByIdAsync(It.IsAny<RoleId>(), It.IsAny<CancellationToken>())).ReturnsAsync((Role?)null);

        var handler = new UpdateRoleCommandHandler(NullLogger<UpdateRoleCommandHandler>.Instance, repo.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.HandleAsync(new UpdateRoleCommand(RoleId.New(), new RoleName("X")), CancellationToken.None));
    }
}

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

public class AssociateClaimToRoleCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenRoleExists_AddsClaim()
    {
        var role = new Role("Admin");
        var repo = new Mock<IRolesRepository>();
        repo.Setup(r => r.GetRoleByIdAsync(role.Id, It.IsAny<CancellationToken>())).ReturnsAsync(role);
        repo.Setup(r => r.AddRoleClaimAsync(role.Id, It.IsAny<RoleClaimType>(), It.IsAny<RoleClaimValue>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new AssociateClaimToRoleCommandHandler(NullLogger<AssociateClaimToRoleCommandHandler>.Instance, repo.Object);

        await handler.HandleAsync(new AssociateClaimToRoleCommand(role.Id, new RoleClaimType("t"), new RoleClaimValue("v")), CancellationToken.None);

        repo.Verify(r => r.AddRoleClaimAsync(role.Id, It.IsAny<RoleClaimType>(), It.IsAny<RoleClaimValue>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRoleNotFound_ThrowsKeyNotFoundException()
    {
        var repo = new Mock<IRolesRepository>();
        repo.Setup(r => r.GetRoleByIdAsync(It.IsAny<RoleId>(), It.IsAny<CancellationToken>())).ReturnsAsync((Role?)null);

        var handler = new AssociateClaimToRoleCommandHandler(NullLogger<AssociateClaimToRoleCommandHandler>.Instance, repo.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.HandleAsync(new AssociateClaimToRoleCommand(RoleId.New(), new RoleClaimType("t"), new RoleClaimValue("v")), CancellationToken.None));
    }
}

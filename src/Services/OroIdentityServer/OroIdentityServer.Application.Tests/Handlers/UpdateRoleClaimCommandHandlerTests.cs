using Moq;
using Xunit;
using System.Threading.Tasks;
using OroIdentityServer.Application.Commands;
using OroIdentityServer.Infraestructure.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using OroIdentityServer.Core.Models;
using System.Threading;
using System;
using System.Collections.Generic;

namespace OroIdentityServer.Application.Tests.Handlers;

public class UpdateRoleClaimCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenRoleClaimExists_UpdatesClaim()
    {
        var claim = new RoleClaim(new RoleClaimType("t"), new RoleClaimValue("v"));
        var repo = new Mock<IRolesRepository>();
        repo.Setup(r => r.GetRoleClaimByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(claim);
        repo.Setup(r => r.UpdateRoleClaimAsync(It.IsAny<Guid>(), It.IsAny<RoleClaimType>(), It.IsAny<RoleClaimValue>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new UpdateRoleClaimCommandHandler(NullLogger<UpdateRoleClaimCommandHandler>.Instance, repo.Object);

        await handler.HandleAsync(new UpdateRoleClaimCommand(new RoleClaimId(Guid.NewGuid()), "t2", "v2"), CancellationToken.None);

        repo.Verify(r => r.UpdateRoleClaimAsync(It.IsAny<Guid>(), It.IsAny<RoleClaimType>(), It.IsAny<RoleClaimValue>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRoleClaimMissing_ThrowsKeyNotFoundException()
    {
        var repo = new Mock<IRolesRepository>();
        repo.Setup(r => r.GetRoleClaimByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((RoleClaim?)null);

        var handler = new UpdateRoleClaimCommandHandler(NullLogger<UpdateRoleClaimCommandHandler>.Instance, repo.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.HandleAsync(new UpdateRoleClaimCommand(new RoleClaimId(Guid.NewGuid()), "t2", "v2"), CancellationToken.None));
    }
}
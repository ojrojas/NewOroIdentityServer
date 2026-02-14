using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using System.Threading.Tasks;
using OroIdentityServer.Application.Commands;
using OroIdentityServer.Infraestructure.Interfaces;
using OroIdentityServer.Core.Models;
using System.Threading;
using System;
using System.Collections.Generic;

namespace OroIdentityServer.Application.Tests.Handlers;

public class DeleteRoleClaimCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenRoleClaimExists_DeletesClaim()
    {
        var rc = new RoleClaim(new RoleClaimType("t"), new RoleClaimValue("v"));
        // generate id via reflection or by EF normally — RoleClaim.Id may be default in tests; handler only needs non-null
        var repo = new Mock<IRolesRepository>();
        repo.Setup(r => r.GetRoleClaimByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(rc);
        repo.Setup(r => r.DeleteRoleClaimAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new DeleteRoleClaimCommandHandler(NullLogger<DeleteRoleClaimCommandHandler>.Instance, repo.Object);

        await handler.HandleAsync(new DeleteRoleClaimCommand(RoleClaimId.New()), CancellationToken.None);

        repo.Verify(r => r.DeleteRoleClaimAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRoleClaimNotFound_ThrowsKeyNotFoundException()
    {
        var repo = new Mock<IRolesRepository>();
        repo.Setup(r => r.GetRoleClaimByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((RoleClaim?)null);

        var handler = new DeleteRoleClaimCommandHandler(NullLogger<DeleteRoleClaimCommandHandler>.Instance, repo.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.HandleAsync(new DeleteRoleClaimCommand(RoleClaimId.New()), CancellationToken.None));
    }
}

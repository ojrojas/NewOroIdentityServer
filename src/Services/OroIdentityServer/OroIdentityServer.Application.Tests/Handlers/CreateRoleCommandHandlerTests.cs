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

public class CreateRoleCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenRoleDoesNotExist_AddsRole()
    {
        var repo = new Mock<IRolesRepository>();
        repo.Setup(r => r.GetRoleByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Role?)null);
        repo.Setup(r => r.AddRoleAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new CreateRoleCommandHandler(NullLogger<CreateRoleCommandHandler>.Instance, repo.Object);

        await handler.HandleAsync(new CreateRoleCommand(new RoleName("Admin")), CancellationToken.None);

        repo.Verify(r => r.AddRoleAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRoleExists_ThrowsInvalidOperationException()
    {
        var existing = new Role("Admin");
        var repo = new Mock<IRolesRepository>();
        repo.Setup(r => r.GetRoleByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var handler = new CreateRoleCommandHandler(NullLogger<CreateRoleCommandHandler>.Instance, repo.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(new CreateRoleCommand(new RoleName("Admin")), CancellationToken.None));
    }
}
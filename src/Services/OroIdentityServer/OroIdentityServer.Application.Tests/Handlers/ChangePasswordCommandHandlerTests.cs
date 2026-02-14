using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using System.Threading.Tasks;
using OroIdentityServer.Infraestructure.Interfaces;
using OroIdentityServer.Application.Commands;
using System;
using System.Threading;

namespace OroIdentityServer.Application.Tests.Handlers;

public class ChangePasswordCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsTrue_Completes()
    {
        var repository = new Mock<IUserRepository>();
        repository.Setup(r => r.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new ChangePasswordCommandHandler(NullLogger<ChangePasswordCommandHandler>.Instance, repository.Object);

        await handler.HandleAsync(new ChangePasswordCommand("a@b.com", "cur", "new", "new"), CancellationToken.None);

        repository.Verify(r => r.ChangePasswordAsync("a@b.com", "cur", "new", "new", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsFalse_Throws()
    {
        var repository = new Mock<IUserRepository>();
        repository.Setup(r => r.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new ChangePasswordCommandHandler(NullLogger<ChangePasswordCommandHandler>.Instance, repository.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(new ChangePasswordCommand("a@b.com", "cur", "new", "new"), CancellationToken.None));
    }
}
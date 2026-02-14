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

public class AddLoginHistoryCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_AddsLoginHistory()
    {
        var repo = new Mock<ILoginHistoryRepository>();
        repo.Setup(r => r.AddLoginHistoryAsync(It.IsAny<LoginHistory>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new AddLoginHistoryCommandHandler(NullLogger<AddLoginHistoryCommandHandler>.Instance, repo.Object);

        var cmd = new AddLoginHistoryCommand(UserId.New(), "127.0.0.1", "Country", DateTime.UtcNow);

        await handler.HandleAsync(cmd, CancellationToken.None);

        repo.Verify(r => r.AddLoginHistoryAsync(It.IsAny<LoginHistory>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_RepositoryThrows_Propagates()
    {
        var repo = new Mock<ILoginHistoryRepository>();
        repo.Setup(r => r.AddLoginHistoryAsync(It.IsAny<LoginHistory>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException());

        var handler = new AddLoginHistoryCommandHandler(NullLogger<AddLoginHistoryCommandHandler>.Instance, repo.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(new AddLoginHistoryCommand(UserId.New(), "127.0.0.1", "Country", DateTime.UtcNow), CancellationToken.None));
    }
}
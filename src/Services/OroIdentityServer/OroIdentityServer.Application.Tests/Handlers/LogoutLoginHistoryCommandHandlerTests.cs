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

public class LogoutLoginHistoryCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenExists_UpdatesLogout()
    {
        var lh = new LoginHistory(null, UserId.New(), "127.0.0.1", "Country", DateTime.UtcNow);
        var repo = new Mock<ILoginHistoryRepository>();
        repo.Setup(r => r.GetLoginHistoryByIdAsync(It.IsAny<LoginHistoryId>(), It.IsAny<CancellationToken>())).ReturnsAsync(lh);
        repo.Setup(r => r.UpdateLoginHistoryAsync(It.IsAny<LoginHistory>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new LogoutLoginHistoryCommandHandler(NullLogger<LogoutLoginHistoryCommandHandler>.Instance, repo.Object);

        await handler.HandleAsync(new LogoutLoginHistoryCommand(lh.Id), CancellationToken.None);

        repo.Verify(r => r.UpdateLoginHistoryAsync(It.Is<LoginHistory>(x => x.LogoutTime.HasValue), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNotFound_Throws()
    {
        var repo = new Mock<ILoginHistoryRepository>();
        repo.Setup(r => r.GetLoginHistoryByIdAsync(It.IsAny<LoginHistoryId>(), It.IsAny<CancellationToken>())).ReturnsAsync((LoginHistory?)null);

        var handler = new LogoutLoginHistoryCommandHandler(NullLogger<LogoutLoginHistoryCommandHandler>.Instance, repo.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(new LogoutLoginHistoryCommand(LoginHistoryId.New()), CancellationToken.None));
    }
}
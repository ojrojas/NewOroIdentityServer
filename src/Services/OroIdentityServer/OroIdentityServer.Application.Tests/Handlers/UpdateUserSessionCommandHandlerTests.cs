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

public class UpdateUserSessionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenSessionExists_UpdatesSession()
    {
        var session = new UserSession(null, UserId.New(), "ref-1", "127.0.0.1", "C", DateTime.UtcNow, DateTime.UtcNow.AddMinutes(30));
        var repo = new Mock<IUserSessionRepository>();
        repo.Setup(r => r.GetUserSessionByRefreshTokenAsync(session.RefreshToken, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        repo.Setup(r => r.UpdateUserSessionAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new UpdateUserSessionCommandHandler(NullLogger<UpdateUserSessionCommandHandler>.Instance, repo.Object);

        await handler.HandleAsync(new UpdateUserSessionCommand(session.RefreshToken, Deactivate: true), CancellationToken.None);

        repo.Verify(r => r.UpdateUserSessionAsync(It.Is<UserSession>(s => !s.IsActive), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenSessionNotFound_Throws()
    {
        var repo = new Mock<IUserSessionRepository>();
        repo.Setup(r => r.GetUserSessionByRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((UserSession?)null);

        var handler = new UpdateUserSessionCommandHandler(NullLogger<UpdateUserSessionCommandHandler>.Instance, repo.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(new UpdateUserSessionCommand("no-token"), CancellationToken.None));
    }
}

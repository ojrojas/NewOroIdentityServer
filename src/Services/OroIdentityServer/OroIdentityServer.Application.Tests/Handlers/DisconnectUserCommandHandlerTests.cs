using Moq;
using Xunit;
using System.Threading.Tasks;
using OroIdentityServer.Application.Commands;
using OroIdentityServer.Infraestructure.Interfaces;
using OroIdentityServer.Core.Models;
using System.Threading;
using System;
using System.Collections.Generic;

namespace OroIdentityServer.Application.Tests.Handlers;

public class DisconnectUserCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenSessionsExist_DeactivatesEach()
    {
        var session1 = new UserSession(null, UserId.New(), "r1", "ip", "c", DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
        var session2 = new UserSession(null, UserId.New(), "r2", "ip", "c", DateTime.UtcNow, DateTime.UtcNow.AddHours(1));

        var repo = new Mock<IUserSessionRepository>();
        repo.Setup(r => r.GetActiveUserSessionsByUserIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<UserSession> {session1, session2});
        repo.Setup(r => r.UpdateUserSessionAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new DisconnectUserCommandHandler(repo.Object);

        await handler.HandleAsync(new DisconnectUserCommand(session1.UserId), CancellationToken.None);

        repo.Verify(r => r.UpdateUserSessionAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task HandleAsync_WhenNoSessions_NoUpdates()
    {
        var repo = new Mock<IUserSessionRepository>();
        repo.Setup(r => r.GetActiveUserSessionsByUserIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<UserSession>());

        var handler = new DisconnectUserCommandHandler(repo.Object);

        await handler.HandleAsync(new DisconnectUserCommand(UserId.New()), CancellationToken.None);

        repo.Verify(r => r.UpdateUserSessionAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
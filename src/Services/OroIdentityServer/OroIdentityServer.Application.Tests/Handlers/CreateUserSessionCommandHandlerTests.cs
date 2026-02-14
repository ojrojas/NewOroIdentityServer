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

public class CreateUserSessionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_AddsUserSession_WhenValid()
    {
        var repo = new Mock<IUserSessionRepository>();
        repo.Setup(r => r.AddUserSessionAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new CreateUserSessionCommandHandler(NullLogger<CreateUserSessionCommandHandler>.Instance, repo.Object);

        var cmd = new CreateUserSessionCommand(UserId.New(), "ref", "127.0.0.1", "Country", DateTime.UtcNow, DateTime.UtcNow.AddHours(1));

        await handler.HandleAsync(cmd, CancellationToken.None);

        repo.Verify(r => r.AddUserSessionAsync(It.Is<UserSession>(s => s.RefreshToken == "ref"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_RepositoryThrows_PropagatesException()
    {
        var repo = new Mock<IUserSessionRepository>();
        repo.Setup(r => r.AddUserSessionAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException());

        var handler = new CreateUserSessionCommandHandler(NullLogger<CreateUserSessionCommandHandler>.Instance, repo.Object);

        var cmd = new CreateUserSessionCommand(UserId.New(), "ref", "127.0.0.1", "Country", DateTime.UtcNow, DateTime.UtcNow.AddHours(1));

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(cmd, CancellationToken.None));
    }
}
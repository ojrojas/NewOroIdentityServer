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

public class UpdateAuthorizationCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenAuthorizationExists_UpdatesAndCallsRepository()
    {
        var app = new Core.Models.Application(null, ClientSecret.New(), new System.Collections.Generic.List<string>{"r"}, new System.Collections.Generic.List<string>{"g"}, new System.Collections.Generic.List<string>{"s"}, TenantId.New());
        var auth = new Core.Models.Authorization(app, DateTime.UtcNow.AddHours(1), "scopes", "active", "type");

        var repo = new Mock<IAuthorizationRepository>();
        repo.Setup(r => r.GetAuthorizationByAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(auth);
        repo.Setup(r => r.UpdateAuthorizationAsync(It.IsAny<Core.Models.Authorization>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new UpdateAuthorizationCommandHandler(NullLogger<UpdateAuthorizationCommandHandler>.Instance, repo.Object);

        await handler.HandleAsync(new UpdateAuthorizationCommand(auth.Id.Value) { NewStatus = "inactive" }, CancellationToken.None);

        repo.Verify(r => r.UpdateAuthorizationAsync(It.Is<Core.Models.Authorization>(a => a.Status == "inactive"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenAuthorizationNotFound_Throws()
    {
        var repo = new Mock<IAuthorizationRepository>();
        repo.Setup(r => r.GetAuthorizationByAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Core.Models.Authorization?)null);

        var handler = new UpdateAuthorizationCommandHandler(NullLogger<UpdateAuthorizationCommandHandler>.Instance, repo.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(new UpdateAuthorizationCommand(Guid.NewGuid()), CancellationToken.None));
    }
}

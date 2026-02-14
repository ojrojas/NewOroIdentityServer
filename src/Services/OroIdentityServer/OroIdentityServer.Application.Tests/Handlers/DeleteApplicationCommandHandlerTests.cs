using Moq;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using OroIdentityServer.Application.Commands;
using OroIdentityServer.Infraestructure.Interfaces;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace OroIdentityServer.Application.Tests.Handlers;

public class DeleteApplicationCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenApplicationExists_Deletes()
    {
        var app = new Core.Models.Application(Core.Models.ApplicationId.New(), Core.Models.ClientSecret.New(), ["u"], ["g"], ["s"], Core.Models.TenantId.New());
        var repo = new Mock<IApplicationRepository>();
        repo.Setup(r => r.GetByIdAsync(app.Id, It.IsAny<CancellationToken>())).ReturnsAsync(app);
        repo.Setup(r => r.DeleteAsync(app, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new DeleteApplicationCommandHandler(NullLogger<DeleteApplicationCommandHandler>.Instance, repo.Object);

        await handler.HandleAsync(new DeleteApplicationCommand(app.Id), CancellationToken.None);

        repo.Verify(r => r.DeleteAsync(app, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenApplicationNotFound_Throws()
    {
        var repo = new Mock<IApplicationRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Core.Models.ApplicationId>(), It.IsAny<CancellationToken>())).ReturnsAsync((Core.Models.Application?)null);

        var handler = new DeleteApplicationCommandHandler(NullLogger<DeleteApplicationCommandHandler>.Instance, repo.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(new DeleteApplicationCommand(Core.Models.ApplicationId.New()), CancellationToken.None));
    }
}

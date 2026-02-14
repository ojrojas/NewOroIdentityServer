using Moq;
using Xunit;
using System.Threading.Tasks;
using OroIdentityServer.Application.Commands;
using OroIdentityServer.Infraestructure.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading;
using System;

namespace OroIdentityServer.Application.Tests.Handlers;

public class CreateAuthorizationCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenApplicationExists_AddsAuthorization()
    {
        var app = new Core.Models.Application(Core.Models.ApplicationId.New(), Core.Models.ClientSecret.New(), new System.Collections.Generic.List<string>{"r"}, new System.Collections.Generic.List<string>{"g"}, new System.Collections.Generic.List<string>{"s"}, Core.Models.TenantId.New());

        var appRepo = new Mock<IApplicationRepository>();
        appRepo.Setup(r => r.GetByIdAsync(It.IsAny<Core.Models.ApplicationId>(), It.IsAny<CancellationToken>())).ReturnsAsync(app);

        var authRepo = new Mock<IAuthorizationRepository>();
        authRepo.Setup(r => r.AddAuthorizationAsync(It.IsAny<Core.Models.Authorization>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new CreateAuthorizationCommandHandler(NullLogger<CreateAuthorizationCommandHandler>.Instance, appRepo.Object, authRepo.Object);

        var cmd = new CreateAuthorizationCommand(app.Id, DateTime.UtcNow.AddHours(1), "s", "active", "type", null);

        await handler.HandleAsync(cmd, CancellationToken.None);

        authRepo.Verify(r => r.AddAuthorizationAsync(It.IsAny<Core.Models.Authorization>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenApplicationMissing_Throws()
    {
        var appRepo = new Mock<IApplicationRepository>();
        appRepo.Setup(r => r.GetByIdAsync(It.IsAny<Core.Models.ApplicationId>(), It.IsAny<CancellationToken>())).ReturnsAsync((Core.Models.Application?)null);

        var authRepo = new Mock<IAuthorizationRepository>();

        var handler = new CreateAuthorizationCommandHandler(NullLogger<CreateAuthorizationCommandHandler>.Instance, appRepo.Object, authRepo.Object);

        var cmd = new CreateAuthorizationCommand(Core.Models.ApplicationId.New(), DateTime.UtcNow.AddHours(1), "s", "active", "type", null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(cmd, CancellationToken.None));
    }
}
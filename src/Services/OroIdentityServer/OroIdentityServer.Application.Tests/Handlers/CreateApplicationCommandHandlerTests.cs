using Moq;
using Xunit;
using System.Threading.Tasks;
using OroIdentityServer.Application.Commands;
using OroIdentityServer.Infraestructure.Interfaces;
using System.Threading;
using System;

namespace OroIdentityServer.Application.Tests.Handlers;

public class CreateApplicationCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenValid_AddsApplicationAndReturnsId()
    {
        var repo = new Mock<IApplicationRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<Core.Models.Application>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new CreateApplicationCommandHandler(repo.Object);

        var dto = new Core.Models.Application(
            Core.Models.ApplicationId.New(),
            Core.Models.ClientSecret.New(),
            new System.Collections.Generic.List<string>{"https://app/cb"},
            new System.Collections.Generic.List<string>{"code"},
            new System.Collections.Generic.List<string>{"scope"},
            Core.Models.TenantId.New());

        var id = await handler.HandleAsync(new CreateApplicationCommand(dto), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id.Value);
        repo.Verify(r => r.AddAsync(It.IsAny<Core.Models.Application>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_InvalidApplication_ThrowsArgumentException()
    {
        var repo = new Mock<IApplicationRepository>();
        var handler = new CreateApplicationCommandHandler(repo.Object);

        // Missing redirect URIs -> Validate will throw
        var dto = new Core.Models.Application(
            Core.Models.ApplicationId.New(),
            Core.Models.ClientSecret.New(),
            new System.Collections.Generic.List<string>(),
            new System.Collections.Generic.List<string>{"code"},
            new System.Collections.Generic.List<string>{"scope"},
            Core.Models.TenantId.New());

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(new CreateApplicationCommand(dto), CancellationToken.None));
    }
}
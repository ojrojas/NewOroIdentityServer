using Moq;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using OroIdentityServer.Application.Commands;
using OroIdentityServer.Infraestructure.Interfaces;
using OroIdentityServer.Core.Models;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace OroIdentityServer.Application.Tests.Handlers;

public class UpdateApplicationCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Valid_CallsUpdate()
    {
        var repo = new Mock<IApplicationRepository>();
        repo.Setup(r => r.UpdateAsync(It.IsAny<Core.Models.Application>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new UpdateApplicationCommandHandler(NullLogger<UpdateApplicationCommandHandler>.Instance, repo.Object);

        var app = new Core.Models.Application(Core.Models.ApplicationId.New(), Core.Models.ClientSecret.New(), new System.Collections.Generic.List<string>{"u"}, new System.Collections.Generic.List<string>{"g"}, new System.Collections.Generic.List<string>{"s"}, Core.Models.TenantId.New());

        await handler.HandleAsync(new UpdateApplicationCommand(app), CancellationToken.None);

        repo.Verify(r => r.UpdateAsync(It.Is<Core.Models.Application>(a => a.Id == app.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_RepositoryThrows_Propagates()
    {
        var repo = new Mock<IApplicationRepository>();
        repo.Setup(r => r.UpdateAsync(It.IsAny<Core.Models.Application>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException());

        var handler = new UpdateApplicationCommandHandler(NullLogger<UpdateApplicationCommandHandler>.Instance, repo.Object);

        var app = new Core.Models.Application(Core.Models.ApplicationId.New(), ClientSecret.New(), new System.Collections.Generic.List<string>{"u"}, new System.Collections.Generic.List<string>{"g"}, new System.Collections.Generic.List<string>{"s"}, TenantId.New());

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(new UpdateApplicationCommand(app), CancellationToken.None));
    }
}

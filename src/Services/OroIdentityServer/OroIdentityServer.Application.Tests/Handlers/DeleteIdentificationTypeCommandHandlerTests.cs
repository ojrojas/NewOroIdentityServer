using Moq;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using OroIdentityServer.Application.Commands;
using OroIdentityServer.Infraestructure.Interfaces;
using OroIdentityServer.Core.Models;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace OroIdentityServer.Application.Tests.Handlers;

public class DeleteIdentificationTypeCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenExists_Deletes()
    {
        var idType = new IdentificationType("IDCard");
        var repo = new Mock<IIdentificationTypeRepository>();
        repo.Setup(r => r.GetIdentificationTypeByIdAsync(idType.Id, It.IsAny<CancellationToken>())).ReturnsAsync(idType);
        repo.Setup(r => r.DeleteIdentificationTypeAsync(idType.Id, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new DeleteIdentificationTypeCommandHandler(NullLogger<DeleteIdentificationTypeCommandHandler>.Instance, repo.Object);

        await handler.HandleAsync(new DeleteIdentificationTypeCommand(idType.Id), CancellationToken.None);

        repo.Verify(r => r.DeleteIdentificationTypeAsync(idType.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNotFound_Throws()
    {
        var repo = new Mock<IIdentificationTypeRepository>();
        repo.Setup(r => r.GetIdentificationTypeByIdAsync(It.IsAny<IdentificationTypeId>(), It.IsAny<CancellationToken>())).ReturnsAsync((IdentificationType?)null);

        var handler = new DeleteIdentificationTypeCommandHandler(NullLogger<DeleteIdentificationTypeCommandHandler>.Instance, repo.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.HandleAsync(new DeleteIdentificationTypeCommand(IdentificationTypeId.New()), CancellationToken.None));
    }
}

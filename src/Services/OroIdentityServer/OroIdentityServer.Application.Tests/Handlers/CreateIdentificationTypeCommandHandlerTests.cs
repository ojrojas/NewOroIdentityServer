using Moq;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using OroIdentityServer.Application.Commands;
using OroIdentityServer.Infraestructure.Interfaces;
using OroIdentityServer.Core.Models;
using System.Threading.Tasks;
using System.Threading;

namespace OroIdentityServer.Application.Tests.Handlers;

public class CreateIdentificationTypeCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Valid_AddsIdentificationType()
    {
        var repo = new Mock<IIdentificationTypeRepository>();
        repo.Setup(r => r.AddIdentificationTypeAsync(It.IsAny<IdentificationType>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new CreateIdentificationTypeCommandHandler(NullLogger<CreateIdentificationTypeCommandHandler>.Instance, repo.Object);

        await handler.HandleAsync(new CreateIdentificationTypeCommand(new IdentificationTypeName("IDCard")), CancellationToken.None);

        repo.Verify(r => r.AddIdentificationTypeAsync(It.IsAny<IdentificationType>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}

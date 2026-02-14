using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using System.Threading.Tasks;
using OroIdentityServer.Application.Commands;
using OroIdentityServer.Infraestructure.Interfaces;
using System.Threading;
using OroIdentityServer.Core.Models;

namespace OroIdentityServer.Application.Tests.Handlers;

public class DeleteRolesByUserIdCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_CallsRepository()
    {
        var repo = new Mock<IUserRolesRepository>();
        repo.Setup(r => r.DeleteRolesByUserIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var handler = new DeleteRolesByUserIdCommandHandler(NullLogger<DeleteRolesByUserIdCommandHandler>.Instance, repo.Object);

        var userId = UserId.New();
        await handler.HandleAsync(new DeleteRolesByUserIdCommand(userId), CancellationToken.None);

        repo.Verify(r => r.DeleteRolesByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

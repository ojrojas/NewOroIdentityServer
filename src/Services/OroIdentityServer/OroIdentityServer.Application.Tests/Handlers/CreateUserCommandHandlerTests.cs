using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using System.Threading.Tasks;
using OroIdentityServer.Infraestructure.Interfaces;
using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Application.Commands;
using OroIdentityServer.Core.Models;
using System.Threading;
using System;

namespace OroIdentityServer.Application.Tests.Handlers;

public class CreateUserCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserDoesNotExist_CreatesUser()
    {
        var userRepository = new Mock<IUserRepository>();
        _ = userRepository.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        userRepository.Setup(r => r.AddUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(p => p.HashPassword(It.IsAny<string>())).ReturnsAsync("hashed");

        var handler = new CreateUserCommandHandler(NullLogger<CreateUserCommandHandler>.Instance, userRepository.Object, passwordHasher.Object);

        var cmd = new CreateUserCommand(
            "Name",
            "Middle",
            "Last",
            "user1",
            "u@d.com",
            "pwd",
            "ident",
            IdentificationTypeId.New(),
            TenantId.New());

        await handler.HandleAsync(cmd, CancellationToken.None);

        userRepository.Verify(r => r.AddUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUserExists_Throws()
    {
        var existing = User.Create("user1", "u@d.com", "Name", "Middle", "Last", "ident", IdentificationTypeId.New(), TenantId.New());
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var passwordHasher = new Mock<IPasswordHasher>();

        var handler = new CreateUserCommandHandler(NullLogger<CreateUserCommandHandler>.Instance, userRepository.Object, passwordHasher.Object);

        var cmd = new CreateUserCommand(
            "Name",
            "Middle",
            "Last",
            "user1",
            "u@d.com",
            "pwd",
            "ident",
            IdentificationTypeId.New(),
            TenantId.New());

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(cmd, CancellationToken.None));
    }
}
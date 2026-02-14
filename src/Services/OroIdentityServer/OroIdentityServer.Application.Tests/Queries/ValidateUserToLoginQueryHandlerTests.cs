using Moq;
using Xunit;
using OroIdentityServer.Application.Queries;
using OroIdentityServer.Infraestructure.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using System.Threading;

namespace OroIdentityServer.Application.Tests.Queries;

public class ValidateUserToLoginQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsFalse_WhenEmailEmpty()
    {
        var repo = new Mock<IUserRepository>();
        var handler = new ValidateUserToLoginQueryHandler(NullLogger<ValidateUserToLoginQuery>.Instance, repo.Object);

        var result = await handler.HandleAsync(new ValidateUserToLoginQuery(string.Empty), CancellationToken.None);
        Assert.False(result);
    }

    [Fact]
    public async Task HandleAsync_DelegatesToRepository_ReturnsValue()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.ValidateUserCanLoginAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new ValidateUserToLoginQueryHandler(NullLogger<ValidateUserToLoginQuery>.Instance, repo.Object);

        var result = await handler.HandleAsync(new ValidateUserToLoginQuery("u@e.com"), CancellationToken.None);
        Assert.True(result);
    }
}
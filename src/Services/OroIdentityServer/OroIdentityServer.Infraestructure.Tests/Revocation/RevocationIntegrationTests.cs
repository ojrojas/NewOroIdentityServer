using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using OroIdentityServer.Core.Models;
using OroIdentityServer.Infraestructure.Repositories;

namespace OroIdentityServer.Infraestructure.Tests.Revocation;

public class RevocationIntegrationTests
{
    [Fact]
    public async Task CreateRevocation_Then_Query_IsRevoked_ReturnsTrue()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var repo = new Repository<RevokedJti>(NullLogger<Repository<RevokedJti>>.Instance, context);

        var jti = "test-jti-123";
        var entity = new RevokedJti(RevokedJtiId.New(), jti, DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
        await repo.AddAsync(entity, CancellationToken.None);

        var found = await repo.FindSingleAsync(x => x.Jti == jti, CancellationToken.None);
        var revoked = found is not null && (found.ExpiresAt == null || found.ExpiresAt > DateTime.UtcNow);

        Assert.True(revoked);
    }

    [Fact]
    public async Task ExpiredRevocation_IsNotReported()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var repo = new Repository<RevokedJti>(NullLogger<Repository<RevokedJti>>.Instance, context);

        var jti = "expired-jti-456";
        var entity = new RevokedJti(RevokedJtiId.New(), jti, DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(-1));
        await repo.AddAsync(entity, CancellationToken.None);

        var found = await repo.FindSingleAsync(x => x.Jti == jti, CancellationToken.None);
        var revoked = found is not null && (found.ExpiresAt == null || found.ExpiresAt > DateTime.UtcNow);

        Assert.False(revoked);
    }
}

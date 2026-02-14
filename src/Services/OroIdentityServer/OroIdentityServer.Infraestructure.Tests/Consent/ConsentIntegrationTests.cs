using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using OroIdentityServer.Core.Models;
using OroIdentityServer.Infraestructure.Repositories;

namespace OroIdentityServer.Infraestructure.Tests;

public class ConsentIntegrationTests
{
    [Fact]
    public async Task CreateAndRetrieveConsent()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var userInfo = Options.Create(new OroKernel.Shared.Options.UserInfo { UserName = "unittest" });
        await using var context = new OroIdentityAppContext(options, userInfo);

        var repo = new Repository<Consent>(NullLogger<Repository<Consent>>.Instance, context);
        var consentRepo = new UserConsentRepository(NullLogger<UserConsentRepository>.Instance, repo);

        var userId = UserId.New();
        var appId = Core.Models.ApplicationId.New();
        var scopes = "openid profile";

        var consent = new Consent(ConsentId.New(), userId, appId, scopes, DateTime.UtcNow, true);
        await consentRepo.AddConsentAsync(consent, CancellationToken.None);

        var found = await consentRepo.GetConsentByUserAndClientAsync(userId, appId, CancellationToken.None);
        Assert.NotNull(found);
        Assert.Equal(scopes, found!.Scopes);
    }
}

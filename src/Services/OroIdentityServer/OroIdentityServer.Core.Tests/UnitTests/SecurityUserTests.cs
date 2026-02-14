using OroIdentityServer.Core.Models;

namespace OroIdentityServer.Core.Tests.UnitTests;

public class SecurityUserTests
{
    [Fact]
    public void Create_WithEmptyPassword_Throws()
    {
        Assert.Throws<ArgumentException>(() => SecurityUser.Create(string.Empty));
    }

    [Fact]
    public void ChangePassword_UpdatesPasswordHashAndConcurrencyStamp()
    {
        var su = SecurityUser.Create("oldhash");
        var oldStamp = su.ConcurrencyStamp;
        su.ChangePassword("newhash");

        Assert.Equal("newhash", su.PasswordHash);
        Assert.NotEqual(oldStamp, su.ConcurrencyStamp);
    }
}
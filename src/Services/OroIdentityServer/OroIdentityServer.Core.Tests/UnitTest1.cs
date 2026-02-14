using OroIdentityServer.Core.Models;

namespace OroIdentityServer.Core.Tests;

public class ModelCreationTests
{
    [Fact]
    public void IdentificationType_Create_WithValidName_SetsProperties()
    {
        var it = IdentificationType.Create("Passport");

        Assert.NotNull(it.Id);
        Assert.Equal("Passport", it.Name.Value);
        Assert.True(it.IsActive);
    }

    [Fact]
    public void IdentificationType_Create_WithEmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() => IdentificationType.Create(string.Empty));
    }

    [Fact]
    public void Tenant_Create_WithValidName_SetsProperties()
    {
        var t = Tenant.Create("Contoso");

        Assert.NotNull(t.Id);
        Assert.Equal("Contoso", t.Name.Value);
        Assert.True(t.IsActive);
    }

    [Fact]
    public void Tenant_Create_WithEmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() => Tenant.Create(""));
    }

    [Fact]
    public void Role_Constructor_WithValidName_SetsProperties()
    {
        var role = new Role("Administrator");

        Assert.NotNull(role.Id);
        Assert.Equal("Administrator", role.Name?.Value);
        Assert.True(role.IsActive);
    }

    [Fact]
    public void Role_Constructor_WithEmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Role(string.Empty));
    }

    [Fact]
    public void User_Create_ValidValues_SetsProperties()
    {
        var idType = IdentificationTypeId.New();
        var tenantId = TenantId.New();

        var user = User.Create(
            "jdoe",
            "jdoe@example.com",
            "John",
            "M",
            "Doe",
            "ID123",
            idType,
            tenantId);

        Assert.NotNull(user.Id);
        Assert.Equal("jdoe", user.UserName);
        Assert.Equal("JDOE", user.NormalizedUserName);
        Assert.Equal("JDOE@EXAMPLE.COM", user.NormalizedEmail);
        Assert.Equal(idType, user.IdentificationTypeId);
        Assert.Equal(tenantId, user.TenantId);
        Assert.Empty(user.Roles);
        Assert.Null(user.SecurityUser);
    }

    [Fact]
    public void User_Create_EmptyUserName_Throws()
    {
        var idType = IdentificationTypeId.New();
        var tenantId = TenantId.New();

        Assert.Throws<ArgumentException>(() => User.Create(string.Empty, "e@d.com", "N", "M", "L", "id", idType, tenantId));
    }

    [Fact]
    public void User_Create_EmptyEmail_Throws()
    {
        var idType = IdentificationTypeId.New();
        var tenantId = TenantId.New();

        Assert.Throws<ArgumentException>(() => User.Create("u1", string.Empty, "N", "M", "L", "id", idType, tenantId));
    }

    [Fact]
    public void Application_Constructor_WithValidArguments_SetsProperties()
    {
        var secret = ClientSecret.New();
        var rUris = new List<string>{ "https://app/cb" };
        var grants = new List<string>{ "authorization_code" };
        var scopes = new List<string>{ "openid" };
        var tenant = TenantId.New();

        var app = new Application(null, secret, rUris, grants, scopes, tenant);

        Assert.NotNull(app.Id);
        Assert.Equal(secret, app.ClientSecret);
        Assert.Equal(rUris, app.RedirectUris);
        Assert.Equal(grants, app.GrantTypes);
        Assert.Equal(scopes, app.Scopes);
        Assert.Equal(tenant, app.TenantId);
        Assert.True(app.IsActive);
    }

    [Fact]
    public void Scope_Constructor_WithValidArguments_SetsProperties()
    {
        var scope = new Scope(ScopeId.New(), ScopeName.Create("read"), "Read", "Read scope", TenantId.New());

        Assert.NotNull(scope.Id);
        Assert.Equal("read", scope.Name.Value);
        Assert.True(scope.IsActive);
    }

    [Fact]
    public void LoginHistory_Constructor_And_Logout_Works()
    {
        var loginTime = DateTime.UtcNow;
        var lh = new LoginHistory(null, UserId.New(), "127.0.0.1", "Country", loginTime);

        Assert.NotNull(lh.Id);
        Assert.Equal("127.0.0.1", lh.IpAddress);
        Assert.True(lh.IsActive);
        Assert.Null(lh.LogoutTime);

        lh.Logout();
        Assert.False(lh.IsActive);
        Assert.NotNull(lh.LogoutTime);
    }

    [Fact]
    public void UserSession_Constructor_Deactivate_And_IsExpired()
    {
        var loginTime = DateTime.UtcNow.AddMinutes(-10);
        var expiresAt = DateTime.UtcNow.AddMinutes(-1);
        var us = new UserSession(null, UserId.New(), "ref-1", "127.0.0.1", "Country", loginTime, expiresAt);

        Assert.True(us.IsActive);
        Assert.True(us.IsExpired());

        us.Deactivate();
        Assert.False(us.IsActive);
    }

    [Fact]
    public void IdentificationTypeName_Create_And_ScopeName_Create_Validate()
    {
        var itn = IdentificationTypeName.Create("TypeA");
        Assert.Equal("TypeA", itn.Value);

        Assert.Throws<ArgumentNullException>(() => IdentificationTypeName.Create(string.Empty));
        Assert.Throws<ArgumentNullException>(() => ScopeName.Create(string.Empty));
        Assert.Throws<ArgumentNullException>(() => TenantName.Create(string.Empty));
    }
}


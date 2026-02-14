// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Models;
using System.Security.Cryptography;
using System.Text;

public sealed class Application : BaseEntity<Application, ApplicationId>, IAuditableEntity, IAggregateRoot
{
    public Application(
        ApplicationId? id,
        ClientSecret clientSecret,
        List<string> redirectUris,
        List<string> grantTypes,
        List<string> scopes,
        TenantId tenantId)
    {
        Id = id ?? ApplicationId.New();
        ClientSecret = clientSecret ?? ClientSecret.New();
        RedirectUris = redirectUris;
        GrantTypes = grantTypes;
        Scopes = scopes;
        TenantId = tenantId;
        IsActive = true;
        RaiseDomainEvent(new ApplicationCreateEvent(Id, tenantId));
    }

    private Application() { }

    public ClientSecret ClientSecret { get; private set; } = ClientSecret.New();
    public string? ClientSecretHash { get; private set; }
    public List<string> RedirectUris { get; private set; } = [];
    public List<string> GrantTypes { get; private set; } = [];
    public List<string> Scopes { get; private set; } = [];
    public TenantId TenantId { get; private set; }
    public bool IsActive { get; private set; }

    public Tenant? Tenant { get; set; }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        RaiseDomainEvent(new ApplicationDeactivateEvent(Id));
    }

    public void Validate()
    {
        if (Id == null || Guid.Empty == Id.Value)
            throw new ArgumentException("ApplicationId cannot be empty.");
        if (ClientSecret == null || Guid.Empty == ClientSecret.Value)
            throw new ArgumentException("ClientSecret cannot be empty.");
        if (RedirectUris == null || !RedirectUris.Any())
            throw new ArgumentException("At least one redirect URI is required.");
    }

    public bool ValidateClientSecret(string presentedSecret)
    {
        if (string.IsNullOrEmpty(presentedSecret)) return false;

        if (!string.IsNullOrEmpty(ClientSecretHash))
        {
            return OroIdentityServer.Core.Security.SecretHasher.Verify(presentedSecret, ClientSecretHash);
        }

        try
        {
            var stored = ClientSecret.Value.ToString();
            return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(stored), Encoding.UTF8.GetBytes(presentedSecret));
        }
        catch
        {
            return false;
        }
    }

    public void SetClientSecretHash(string hash)
    {
        ClientSecretHash = hash;
    }
}
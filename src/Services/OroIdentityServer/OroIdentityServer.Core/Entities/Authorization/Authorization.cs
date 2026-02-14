// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Models;

public class Authorization : BaseEntity<Authorization, AuthorizationId>, IAuditableEntity, IAggregateRoot
{
    public Application Application { get; private set; }
    public string ConcurrencyToken { get; private set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; private set; }
    [StringSyntax(StringSyntaxAttribute.Json)]
    public virtual string? Properties { get; set; }
    public string Scopes { get; private set; }
    public string Status { get; private set; }
    public string Type { get; private set; }

    public Authorization(
        Application application,
        DateTime expiresAt,
        string scopes,
        string status,
        string type,
        string? properties = null)
    {
        Id = AuthorizationId.New();
        Application = application;
        ExpiresAt = expiresAt;
        Scopes = scopes;
        Status = status;
        Type = type;
        Properties = properties;
            RaiseDomainEvent(new AuthorizationCreatedEvent(Id, application.Id));
    }

        private Authorization() { }

    public void UpdateStatus(string newStatus)
    {
        if (Status == newStatus) return;
        Status = newStatus;
        ConcurrencyToken = Guid.NewGuid().ToString();
        RaiseDomainEvent(new AuthorizationUpdatedEvent(Id, Application.Id));
    }

    public void UpdateProperties(string? newProperties)
    {
        if (Properties == newProperties) return;
        Properties = newProperties;
        ConcurrencyToken = Guid.NewGuid().ToString();
        RaiseDomainEvent(new AuthorizationUpdatedEvent(Id, Application.Id));
    }

    public void Deactivate()
    {
        if (Status == "inactive") return;
        Status = "inactive";
        ConcurrencyToken = Guid.NewGuid().ToString();
        RaiseDomainEvent(new AuthorizationDeactivatedEvent(Id, Application.Id));
    }   

    public void Activate()
    {
        if (Status == "active") return;
        Status = "active";
        ConcurrencyToken = Guid.NewGuid().ToString();
        RaiseDomainEvent(new AuthorizationActivatedEvent(Id, Application.Id));
    }   

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
}
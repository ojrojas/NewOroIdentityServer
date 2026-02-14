// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Models;

public sealed class Scope : BaseEntity<Scope, ScopeId>, IAuditableEntity, IAggregateRoot
{
    public string? ConcurrencyToken { get; private set; } = Guid.NewGuid().ToString();
    public ScopeName Name { get; private set; }
    public string? DisplayName { get; private set; }
    public string? Description { get; private set; }
    [StringSyntax(StringSyntaxAttribute.Json)]
    public string? Descriptions { get; set; }
    public TenantId TenantId { get; private set; }
    [StringSyntax(StringSyntaxAttribute.Json)]
    public string? Properties { get; set; }
    [StringSyntax(StringSyntaxAttribute.Json)]
    public string? Resources { get; set; }

    public bool IsActive { get; private set; }

    public Tenant? Tenant { get; set; }

    public Scope(
        ScopeId id,
        ScopeName name,
        string? displayName,
        string? description,
        TenantId tenantId,
        string? descriptions = null,
        string? properties = null,
        string? resources = null)
    {
        Id = id;
        Name = name;
        DisplayName = displayName;
        Description = description;
        Descriptions = descriptions;
        Properties = properties;
        Resources = resources;
        TenantId = tenantId;
        IsActive = true;
        RaiseDomainEvent(new ScopeCreatedEvent(Id));
    }

    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        RaiseDomainEvent(new ScopeActivatedEvent(Id));
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        RaiseDomainEvent(new ScopeDeactivatedEvent(Id));
    }

    public void UpdateName(ScopeName newName)
    {
        if (newName == null || string.IsNullOrWhiteSpace(newName.Value))
            throw new ArgumentException("New name cannot be null or empty.");

        if (Name != null && Name.Equals(newName)) return; // Avoid unnecessary updates

        Name = newName;
        RaiseDomainEvent(new ScopeUpdatedEvent(Id));
    }

    public void Validate()
    {
        if (Name == null || string.IsNullOrWhiteSpace(Name.Value))
            throw new ArgumentException("Scope name cannot be empty.");
    }
}

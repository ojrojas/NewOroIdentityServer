// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure;

public class OroIdentityAppContext(
    DbContextOptions<OroIdentityAppContext> options, IOptions<UserInfo> optionsUser) : AuditableDbContext(options, optionsUser)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<SecurityUser> SecurityUsers { get; set; }
    public DbSet<IdentificationType> IdentificationTypes { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Tenant> Tenants {get;set;}
    public DbSet<Application> Applications { get; set; }
    public DbSet<Authorization> Authorizations { get; set; }
    public DbSet<Token> Tokens { get; set; }
    public DbSet<Scope> Scopes { get; set; }
    public DbSet<LoginHistory> LoginHistories { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<RevokedJti> RevokedJtis { get; set; }
    public DbSet<Consent> Consents { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        
        builder.ApplyConfiguration(new UserEntityConfiguration());
        builder.ApplyConfiguration(new RoleEntityConfiguration());
        builder.ApplyConfiguration(new IdentificationTypeEntityConfiguration());
        builder.ApplyConfiguration(new UserRoleEntityConfiguration());
        builder.ApplyConfiguration(new SecurityUserEntityConfiguration());
        builder.ApplyConfiguration(new TenantEntityConfiguration());
        builder.ApplyConfiguration(new ApplicationEntityConfiguration());
        builder.ApplyConfiguration(new AuthorizationEntityConfiguration());
        builder.ApplyConfiguration(new TokenEntityConfiguration());
        builder.ApplyConfiguration(new ScopeEntityConfiguration());
        builder.ApplyConfiguration(new LoginHistoryEntityConfiguration());
        builder.ApplyConfiguration(new UserSessionEntityConfiguration());
        builder.ApplyConfiguration(new RevokedJtiEntityConfiguration());
        builder.ApplyConfiguration(new ConsentEntityConfiguration());
    }
}
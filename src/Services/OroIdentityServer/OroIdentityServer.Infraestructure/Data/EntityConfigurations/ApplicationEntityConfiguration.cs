// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data.EntityConfigurations;

public class ApplicationEntityConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.ToTable("Applications");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => new Core.Models.ApplicationId(value))
            .HasColumnName("Id");

        builder.Property(a => a.ClientSecret)
            .HasConversion(clientId => clientId.Value, value => new ClientSecret(value))
            .HasColumnName("ClientSecret")
            .IsRequired();

        builder.Property(a => a.ClientSecretHash)
            .HasColumnName("ClientSecretHash");

        builder.Property(a => a.RedirectUris)
            .HasColumnName("RedirectUris");

        builder.Property(a => a.GrantTypes)
            .HasColumnName("GrantTypes");

        builder.Property(a => a.Scopes)
            .HasColumnName("Scopes");

        builder.Property(a => a.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .HasColumnName("TenantId");

        builder.HasOne(a => a.Tenant)
            .WithMany()
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(a => a.IsActive)
            .HasColumnName("IsActive")
            .IsRequired();

        builder.HasIndex(a => a.TenantId)
            .HasDatabaseName("IX_Applications_TenantId");

        builder.HasQueryFilter(a => a.IsActive);
    }
}
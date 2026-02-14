// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data.EntityConfigurations;

public class ScopeEntityConfiguration : IEntityTypeConfiguration<Scope>
{
    public void Configure(EntityTypeBuilder<Scope> builder)
    {
        builder.ToTable("Scopes");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => new ScopeId(value))
            .HasColumnName("Id");

        builder.Property(s => s.ConcurrencyToken)
            .HasMaxLength(50)
            .IsConcurrencyToken();

        builder.Property(s => s.Name)
            .HasConversion(name => name.Value, value => new ScopeName(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.DisplayName)
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        builder.Property(s => s.Descriptions)
            .HasColumnType("jsonb");

        builder.Property(s => s.Properties)
            .HasColumnType("jsonb");

        builder.Property(s => s.Resources)
            .HasColumnType("jsonb");

        builder.HasOne(s => s.Tenant)
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
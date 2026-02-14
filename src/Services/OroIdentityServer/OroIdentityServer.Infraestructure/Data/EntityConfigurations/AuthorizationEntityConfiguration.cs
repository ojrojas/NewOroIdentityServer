// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data.EntityConfigurations;

public class AuthorizationEntityConfiguration : IEntityTypeConfiguration<Authorization>
{
    public void Configure(EntityTypeBuilder<Authorization> builder)
    {
        builder.ToTable("Authorizations");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => new AuthorizationId(value))
            .HasColumnName("Id");

        builder.Property(a => a.ConcurrencyToken)
            .HasMaxLength(50)
            .IsConcurrencyToken();

        builder.Property(a => a.CreatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(a => a.ExpiresAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(a => a.Properties)
            .HasColumnType("jsonb");

        builder.Property(a => a.Scopes)
            .HasMaxLength(1000);

        builder.Property(a => a.Status)
            .HasMaxLength(50);

        builder.Property(a => a.Type)
            .HasMaxLength(50);

        builder.HasOne(a => a.Application)
            .WithMany()
            .HasForeignKey("ApplicationId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data.EntityConfigurations;

public class TokenEntityConfiguration : IEntityTypeConfiguration<Token>
{
    public void Configure(EntityTypeBuilder<Token> builder)
    {
        builder.ToTable("Tokens");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => new TokenId(value))
            .HasColumnName("Id");

        builder.Property(t => t.ConcurrencyToken)
            .HasMaxLength(50)
            .IsConcurrencyToken();

        builder.Property(t => t.CreationDate)
            .HasColumnType("timestamp with time zone");

        builder.Property(t => t.ExpirationDate)
            .HasColumnType("timestamp with time zone");

        builder.Property(t => t.Payload)
            .HasColumnType("jsonb");

        builder.Property(t => t.Properties)
            .HasColumnType("jsonb");

        builder.Property(t => t.RedemptionDate)
            .HasColumnType("timestamp with time zone");

        builder.Property(t => t.ReferenceId)
            .HasMaxLength(100);

        builder.Property(t => t.Status)
            .HasMaxLength(50);

        builder.Property(t => t.Subject)
            .HasMaxLength(400);

        builder.Property(t => t.Type)
            .HasMaxLength(50);

        builder.HasOne(t => t.Application)
            .WithMany()
            .HasForeignKey("ApplicationId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Authorization)
            .WithMany()
            .HasForeignKey("AuthorizationId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
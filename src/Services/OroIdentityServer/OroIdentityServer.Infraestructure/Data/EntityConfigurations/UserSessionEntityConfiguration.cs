// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data.EntityConfigurations;

public class UserSessionEntityConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.HasKey(us => us.Id);
        builder.Property(us => us.Id).HasConversion(id => id.Value, value => new UserSessionId(value));
        builder.Property(us => us.UserId).HasConversion(id => id.Value, value => new UserId(value));
        builder.Property(us => us.RefreshToken).IsRequired().HasMaxLength(500);
        builder.Property(us => us.IpAddress).IsRequired().HasMaxLength(45);
        builder.Property(us => us.Country).HasMaxLength(100);
        builder.Property(us => us.LoginTime).IsRequired();
        builder.Property(us => us.ExpiresAt).IsRequired();
        builder.Property(us => us.IsActive).IsRequired();
        builder.HasOne(us => us.User).WithMany().HasForeignKey(us => us.UserId);
    }
}
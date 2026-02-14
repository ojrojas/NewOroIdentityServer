// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data.EntityConfigurations;

public class LoginHistoryEntityConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        builder.HasKey(lh => lh.Id);
        builder.Property(lh => lh.Id).HasConversion(id => id.Value, value => new LoginHistoryId(value));
        builder.Property(lh => lh.UserId).HasConversion(id => id.Value, value => new UserId(value));
        builder.Property(lh => lh.IpAddress).IsRequired().HasMaxLength(45);
        builder.Property(lh => lh.Country).HasMaxLength(100);
        builder.Property(lh => lh.LoginTime).IsRequired();
        builder.Property(lh => lh.LogoutTime);
        builder.Property(lh => lh.IsActive).IsRequired();
        builder.HasOne(lh => lh.User).WithMany().HasForeignKey(lh => lh.UserId);
    }
}
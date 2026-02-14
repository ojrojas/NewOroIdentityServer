namespace OroIdentityServer.Infraestructure.Data.EntityConfigurations;

public class RevokedJtiEntityConfiguration : IEntityTypeConfiguration<RevokedJti>
{
    public void Configure(EntityTypeBuilder<RevokedJti> builder)
    {
        builder.ToTable("RevokedJtis");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => new RevokedJtiId(value))
            .HasColumnName("Id");
        builder.Property(r => r.Jti).HasMaxLength(256).IsRequired();
        builder.Property(r => r.RevokedAt).IsRequired();
        builder.Property(r => r.ExpiresAt).IsRequired(false);
    }
}

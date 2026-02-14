namespace OroIdentityServer.Infraestructure.Data.EntityConfigurations;

public class ConsentEntityConfiguration : IEntityTypeConfiguration<Consent>
{
    public void Configure(EntityTypeBuilder<Consent> builder)
    {
        builder.ToTable("Consents");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasConversion(id => id.Value, value => new ConsentId(value));
        builder.Property(c => c.UserId).HasConversion(id => id.Value, value => new UserId(value));
        builder.Property(c => c.ApplicationId).HasConversion(id => id.Value, value => new Core.Models.ApplicationId(value));
        builder.Property(c => c.Scopes).HasMaxLength(1000).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.Remember).IsRequired();
    }
}

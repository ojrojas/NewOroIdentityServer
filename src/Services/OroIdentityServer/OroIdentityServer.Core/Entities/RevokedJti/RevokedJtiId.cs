namespace OroIdentityServer.Core.Models;

public sealed class RevokedJtiId(Guid value) : BaseValueObject
{
    public Guid Value { get; private set; } = value;

    public static RevokedJtiId New() => new(Guid.CreateVersion7());

    protected override IEnumerable<object?> GetEquatibilityComponents()
    {
        yield return Value;
    }
}
